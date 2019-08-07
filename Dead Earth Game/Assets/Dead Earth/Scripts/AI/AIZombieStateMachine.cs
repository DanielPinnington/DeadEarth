using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum AIBoneControlType { Animated, Ragdoll, RagdollToAnim }
public enum AIScreamPosition { Entity, Player }

// ----------------------------------------------------------------
// Class	:	BodyPartSnapshot
// Desc		:   Used to store information about the positions of
//				each body part when transitioning from a 
//				ragdoll
// ----------------------------------------------------------------
public class BodyPartSnapshot
{
    public Transform transform;
    public Vector3 position;
    public Quaternion rotation;
}

// --------------------------------------------------------------------------
// CLASS	:	AIZombieStateMachine
// DESC		:	State Machine used by zombie characters
// --------------------------------------------------------------------------
public class AIZombieStateMachine : AIStateMachine
{
    // Inspector Assigned
    [SerializeField] [Range(10.0f, 360.0f)] float _fov = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _sight = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _hearing = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _aggression = 0.5f;
    [SerializeField] [Range(0, 100)] int _health = 100;
    [SerializeField] [Range(0, 100)] int _lowerBodyDamage = 0;
    [SerializeField] [Range(0, 100)] int _upperBodyDamage = 0;
    [SerializeField] [Range(0, 100)] int _upperBodyThreshold = 30;
    [SerializeField] [Range(0, 100)] int _limpThreshold = 30;
    [SerializeField] [Range(0, 100)] int _crawlThreshold = 90;
    [SerializeField] [Range(0.0f, 1.0f)] float _intelligence = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _satisfaction = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _screamChance = 1.0f;
    [SerializeField] [Range(0.0f, 50.0f)] float _screamRadius = 20.0f;
    [SerializeField] AIScreamPosition _screamPosition = AIScreamPosition.Entity;
    [SerializeField] AISoundEmitter _screamPrefab = null;
    [SerializeField] float _replenishRate = 0.5f;
    [SerializeField] float _depletionRate = 0.1f;
    [SerializeField] float _reanimationBlendTime = 1.5f;
    [SerializeField] float _reanimationWaitTime = 3.0f;
    [SerializeField] LayerMask _geometryLayers = 0;


    // Private
    private int _seeking = 0;
    private bool _feeding = false;
    private bool _crawling = false;
    private int _attackType = 0;
    private float _speed = 0.0f;
    private float _isScreaming = 0.0f;

    // Ragdoll Stuff
    private AIBoneControlType _boneControlType = AIBoneControlType.Animated;
    private List<BodyPartSnapshot> _bodyPartSnapShots = new List<BodyPartSnapshot>();
    private float _ragdollEndTime = float.MinValue;
    private Vector3 _ragdollHipPosition;
    private Vector3 _ragdollFeetPosition;
    private Vector3 _ragdollHeadPosition;
    private IEnumerator _reanimationCoroutine = null;
    private float _mecanimTransitionTime = 0.1f;

    // Hashes
    private int _speedHash = Animator.StringToHash("Speed");
    private int _seekingHash = Animator.StringToHash("Seeking");
    private int _feedingHash = Animator.StringToHash("Feeding");
    private int _attackHash = Animator.StringToHash("Attack");
    private int _crawlingHash = Animator.StringToHash("Crawling");
    private int _screamingHash = Animator.StringToHash("Screaming");
    private int _screamHash = Animator.StringToHash("Scream");
    private int _hitTriggerHash = Animator.StringToHash("Hit");
    private int _hitTypeHash = Animator.StringToHash("HitType");
    private int _lowerBodyDamageHash = Animator.StringToHash("Lower Body Damage");
    private int _upperBodyDamageHash = Animator.StringToHash("Upper Body Damage");
    private int _reanimateFromBackHash = Animator.StringToHash("Reanimate From Back");
    private int _reanimateFromFrontHash = Animator.StringToHash("Reanimate From Front");
    private int _stateHash = Animator.StringToHash("State");
    private int _upperBodyLayer = -1;
    private int _lowerBodyLayer = -1;

    // Public Properties
    public float replenishRate { get { return _replenishRate; } }
    public float fov { get { return _fov; } }
    public float hearing { get { return _hearing; } }
    public float sight { get { return _sight; } }
    public bool crawling { get { return _crawling; } }
    public float intelligence { get { return _intelligence; } }
    public float satisfaction { get { return _satisfaction; } set { _satisfaction = value; } }
    public float aggression { get { return _aggression; } set { _aggression = value; } }
    public int health { get { return _health; } set { _health = value; } }
    public int attackType { get { return _attackType; } set { _attackType = value; } }
    public bool feeding { get { return _feeding; } set { _feeding = value; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }
    public bool isCrawling
    {
        get { return (_lowerBodyDamage >= _crawlThreshold); }
    }

    public bool isScreaming
    {
        get { return _isScreaming > 0.1f; }
    }

    // Set the Trigger to cause screaming
    public bool Scream()
    {
        if (isScreaming) return true;
        if (_animator == null || IsLayerActive("Cinematic") || _screamPrefab == null) return false;

        _animator.SetTrigger(_screamHash);
        Vector3 spawnPos = _screamPosition == AIScreamPosition.Entity ? transform.position : VisualThreat.position;
        AISoundEmitter screamEmitter = Instantiate(_screamPrefab, spawnPos, Quaternion.identity) as AISoundEmitter;

        if (screamEmitter != null)
            screamEmitter.SetRadius(_screamRadius);
        return true;
    }

    // What is the scream chance of this zombie
    public float screamChance
    {
        get { return _screamChance; }
    }

    protected override void Start()
    {
        base.Start();

        if (_animator != null)
        {
            // Cache Layer Indices
            _lowerBodyLayer = _animator.GetLayerIndex("Lower Body");
            _upperBodyLayer = _animator.GetLayerIndex("Upper Body");
        }

        // Create BodyPartSnapShot List
        if (_rootBone != null)
        {
            Transform[] transforms = _rootBone.GetComponentsInChildren<Transform>();
            foreach (Transform trans in transforms)
            {
                BodyPartSnapshot snapShot = new BodyPartSnapshot();
                snapShot.transform = trans;
                _bodyPartSnapShots.Add(snapShot);
            }
        }

        UpdateAnimatorDamage();
    }

    // ---------------------------------------------------------
    // Name	:	Update
    // Desc	:	Refresh the animator with up-to-date values for
    //			its parameters
    // ---------------------------------------------------------
    protected override void Update()
    {
        base.Update();

        if (_animator != null)
        {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetBool(_feedingHash, _feeding);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
            _animator.SetInteger(_stateHash, (int)_currentStateType);

            // Are we screaming or not
            _isScreaming = IsLayerActive("Cinematic") ? 0.0f : _animator.GetFloat(_screamingHash);

        }

        _satisfaction = Mathf.Max(0, _satisfaction - ((_depletionRate * Time.deltaTime) / 100.0f) * Mathf.Pow(_speed, 3.0f));
    }

    protected void UpdateAnimatorDamage()
    {
        if (_animator != null)
        {
            if (_lowerBodyLayer != -1)
            {
                _animator.SetLayerWeight(_lowerBodyLayer, (_lowerBodyDamage > _limpThreshold && _lowerBodyDamage < _crawlThreshold) ? 1.0f : 0.0f);
            }

            if (_upperBodyLayer != -1)
            {
                _animator.SetLayerWeight(_upperBodyLayer, (_upperBodyDamage > _upperBodyThreshold && _lowerBodyDamage < _crawlThreshold) ? 1.0f : 0.0f);
            }

            _animator.SetBool(_crawlingHash, isCrawling);
            _animator.SetInteger(_lowerBodyDamageHash, _lowerBodyDamage);
            _animator.SetInteger(_upperBodyDamageHash, _upperBodyDamage);

            if (_lowerBodyDamage > _limpThreshold && _lowerBodyDamage < _crawlThreshold)
                SetLayerActive("Lower Body", true);
            else
                SetLayerActive("Lower Body", false);

            if (_upperBodyDamage > _upperBodyThreshold && _lowerBodyDamage < _crawlThreshold)
                SetLayerActive("Upper Body", true);
            else
                SetLayerActive("Upper Body", false);

        }
    }

    // -------------------------------------------------------------------
    // Name	:	TakeDamage
    // Desc	:	Processes the zombie's reaction to being dealt damage
    // --------------------------------------------------------------------
    public override void TakeDamage(Vector3 position, Vector3 force, int damage, Rigidbody bodyPart, CharacterManager characterManager, int hitDirection = 0)
    {
        if (GameSceneManager.instance != null && GameSceneManager.instance.bloodParticles != null)
        {
            ParticleSystem sys = GameSceneManager.instance.bloodParticles;

            if (sys)
            {
                sys.transform.position = position;
                var settings = sys.main;
                settings.simulationSpace = ParticleSystemSimulationSpace.World;
                sys.Emit(60);
            }
        }

        float hitStrength = force.magnitude;

        if (_boneControlType == AIBoneControlType.Ragdoll)
        {
            if (bodyPart != null)
            {
                if (hitStrength > 1.0f)
                    bodyPart.AddForce(force, ForceMode.Impulse);


                if (bodyPart.CompareTag("Head"))
                {
                    _health = Mathf.Max(_health - damage, 0);
                }
                else
                if (bodyPart.CompareTag("Upper Body"))
                {
                    _upperBodyDamage += damage;
                }
                else
                if (bodyPart.CompareTag("Lower Body"))
                {
                    _lowerBodyDamage += damage;
                }

                UpdateAnimatorDamage();

                if (_health > 0)
                {
                    if (_reanimationCoroutine != null)
                        StopCoroutine(_reanimationCoroutine);

                    _reanimationCoroutine = Reanimate();
                    StartCoroutine(_reanimationCoroutine);
                }
            }

            return;
        }

        // Get local space position of attacker
        Vector3 attackerLocPos = transform.InverseTransformPoint(characterManager.transform.position);

        bool shouldRagdoll = (hitStrength > 1.0f);

        if (bodyPart != null)
        {
            if (bodyPart.CompareTag("Head"))
            {
                _health = Mathf.Max(_health - damage, 0);
                if (health == 0) shouldRagdoll = true;
            }
            else
            if (bodyPart.CompareTag("Upper Body"))
            {
                _upperBodyDamage += damage;
                UpdateAnimatorDamage();
            }
            else
            if (bodyPart.CompareTag("Lower Body"))
            {
                _lowerBodyDamage += damage;
                UpdateAnimatorDamage();
                shouldRagdoll = true;
            }
        }

        if (_boneControlType != AIBoneControlType.Animated || isCrawling || IsLayerActive("Cinematic") || attackerLocPos.z < 0) shouldRagdoll = true;

        if (!shouldRagdoll)
        {
            float angle = 0.0f;
            if (hitDirection == 0)
            {
                Vector3 vecToHit = (position - transform.position).normalized;
                angle = AIState.FindSignedAngle(vecToHit, transform.forward);
            }

            int hitType = 0;
            if (bodyPart.gameObject.CompareTag("Head"))
            {
                if (angle < -10 || hitDirection == -1) hitType = 1;
                else
                if (angle > 10 || hitDirection == 1) hitType = 3;
                else
                    hitType = 2;
            }
            else
            if (bodyPart.gameObject.CompareTag("Upper Body"))
            {
                if (angle < -20 || hitDirection == -1) hitType = 4;
                else
                if (angle > 20 || hitDirection == 1) hitType = 6;
                else
                    hitType = 5;
            }

            if (_animator)
            {
                _animator.SetInteger(_hitTypeHash, hitType);
                _animator.SetTrigger(_hitTriggerHash);
            }

            return;
        }
        else
        {
            if (_currentState)
            {
                _currentState.OnExitState();
                _currentState = null;
                _currentStateType = AIStateType.None;
            }

            if (_navAgent) _navAgent.enabled = false;
            if (_animator) _animator.enabled = false;
            if (_collider) _collider.enabled = false;

            inMeleeRange = false;

            foreach (Rigidbody body in _bodyParts)
            {
                if (body)
                {
                    body.isKinematic = false;
                }
            }

            if (hitStrength > 1.0f)
            {
                if (bodyPart != null)
                    bodyPart.AddForce(force, ForceMode.Impulse);
            }

            _boneControlType = AIBoneControlType.Ragdoll;

            if (_health > 0)
            {
                if (_reanimationCoroutine != null)
                    StopCoroutine(_reanimationCoroutine);

                _reanimationCoroutine = Reanimate();
                StartCoroutine(_reanimationCoroutine);
            }
        }

    }

    // ----------------------------------------------------------------
    // Name	:	Reanimate (coroutine)
    // Desc	:	Starts the reanimation procedure
    // ----------------------------------------------------------------
    protected IEnumerator Reanimate()
    {
        // Only reanimate if we are in a ragdoll state
        if (_boneControlType != AIBoneControlType.Ragdoll || _animator == null) yield break;

        // Wait for the desired number of seconds before initiating the reanimation process
        yield return new WaitForSeconds(_reanimationWaitTime);

        // Record time at start of reanimation procedure
        _ragdollEndTime = Time.time;

        // Set rigidbodies back to being kinematic
        foreach (Rigidbody body in _bodyParts)
        {
            body.isKinematic = true;
        }

        // Put us in reanimation mode
        _boneControlType = AIBoneControlType.RagdollToAnim;

        // Record postions and rotations of all bones prior to reanimation
        foreach (BodyPartSnapshot snapShot in _bodyPartSnapShots)
        {
            snapShot.position = snapShot.transform.position;
            snapShot.rotation = snapShot.transform.rotation;
        }

        // Record the ragdolls head and feet position
        _ragdollHeadPosition = _animator.GetBoneTransform(HumanBodyBones.Head).position;
        _ragdollFeetPosition = (_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position) * 0.5f;
        _ragdollHipPosition = _rootBone.position;

        // Enable Animator
        _animator.enabled = true;

        if (_rootBone != null)
        {
            float forwardTest;

            switch (_rootBoneAlignment)
            {
                case AIBoneAlignmentType.ZAxis:
                    forwardTest = _rootBone.forward.y; break;
                case AIBoneAlignmentType.ZAxisInverted:
                    forwardTest = -_rootBone.forward.y; break;
                case AIBoneAlignmentType.YAxis:
                    forwardTest = _rootBone.up.y; break;
                case AIBoneAlignmentType.YAxisInverted:
                    forwardTest = -_rootBone.up.y; break;
                case AIBoneAlignmentType.XAxis:
                    forwardTest = _rootBone.right.y; break;
                case AIBoneAlignmentType.XAxisInverted:
                    forwardTest = -_rootBone.right.y; break;
                default:
                    forwardTest = _rootBone.forward.y; break;
            }

            // Set the trigger in the animator
            if (forwardTest >= 0)
                _animator.SetTrigger(_reanimateFromBackHash);
            else
                _animator.SetTrigger(_reanimateFromFrontHash);
        }
    }

    // ---------------------------------------------------------------
    // Name	:	LateUpdate
    // Desc	:	Called by Unity at the end of every frame update. Used
    //			here to perform reanimation.
    // ---------------------------------------------------------------
    protected virtual void LateUpdate()
    {
        if (_boneControlType == AIBoneControlType.RagdollToAnim)
        {
            if (Time.time <= _ragdollEndTime + _mecanimTransitionTime)
            {
                Vector3 animatedToRagdoll = _ragdollHipPosition - _rootBone.position;
                Vector3 newRootPosition = transform.position + animatedToRagdoll;

                RaycastHit[] hits = Physics.RaycastAll(newRootPosition + (Vector3.up * 0.25f), Vector3.down, float.MaxValue, _geometryLayers);
                newRootPosition.y = float.MinValue;
                foreach (RaycastHit hit in hits)
                {
                    if (!hit.transform.IsChildOf(transform))
                    {
                        newRootPosition.y = Mathf.Max(hit.point.y, newRootPosition.y);
                    }
                }

                NavMeshHit navMeshHit;
                Vector3 baseOffset = Vector3.zero;
                if (_navAgent) baseOffset.y = _navAgent.baseOffset;
                if (NavMesh.SamplePosition(newRootPosition, out navMeshHit, 25.0f, NavMesh.AllAreas))
                {
                    transform.position = navMeshHit.position + baseOffset;
                }
                else
                {
                    transform.position = newRootPosition + baseOffset;
                }


                Vector3 ragdollDirection = _ragdollHeadPosition - _ragdollFeetPosition;
                ragdollDirection.y = 0.0f;

                Vector3 meanFeetPosition = 0.5f * (_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                Vector3 animatedDirection = _animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
                animatedDirection.y = 0.0f;

                //Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
                //hence setting the y components of the vectors to zero. 
                transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdollDirection.normalized);
            }

            // Calculate Interpolation value
            float blendAmount = Mathf.Clamp01((Time.time - _ragdollEndTime - _mecanimTransitionTime) / _reanimationBlendTime);

            // Calculate blended bone positions by interplating between ragdoll bone snapshots and animated bone positions
            foreach (BodyPartSnapshot snapshot in _bodyPartSnapShots)
            {
                if (snapshot.transform == _rootBone)
                {
                    snapshot.transform.position = Vector3.Lerp(snapshot.position, snapshot.transform.position, blendAmount);
                }

                snapshot.transform.rotation = Quaternion.Slerp(snapshot.rotation, snapshot.transform.rotation, blendAmount);
            }


            // Conditional to exit reanimation mode
            if (blendAmount == 1.0f)
            {
                _boneControlType = AIBoneControlType.Animated;
                if (_navAgent) _navAgent.enabled = true;
                if (_collider) _collider.enabled = true;

                AIState newState = null;
                if (_states.TryGetValue(AIStateType.Alerted, out newState))
                {
                    if (_currentState != null) _currentState.OnExitState();
                    newState.OnEnterState();
                    _currentState = newState;
                    _currentStateType = AIStateType.Alerted;
                }
            }

        }
    }
}
