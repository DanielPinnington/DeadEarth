using UnityEngine;
using System.Collections;

public class CharacterManager : MonoBehaviour
{
    // Inspector Assigned
    [SerializeField] private CapsuleCollider _meleeTrigger = null;
    [SerializeField] private CameraBloodEffect _cameraBloodEffect = null;
    [SerializeField] private Camera _camera = null;
    [SerializeField] private float _health = 100.0f;
    [SerializeField] private int _ammo = 6;
    [SerializeField] private int _maxAmmo = 6;
    [SerializeField] private AISoundEmitter _soundEmitter = null;
    [SerializeField] private float _runRadius = 7.0f;
    [SerializeField] private float _walkRadius = 0.0f;
    [SerializeField] private float _landingRadius = 12.0f;
    [SerializeField] private float _bloodRadiusScale = 6.0f;
    [SerializeField] private PlayerHud _playerHUD = null;

    //Pain Damage Audio
    [SerializeField] private AudioCollection _damageSounds      = null;
    [SerializeField] private AudioCollection _painSounds        = null;
    [SerializeField] private float           _nextPainSoundTime = 0.0f;
    [SerializeField] private float           _painSoundOffset   = 0.35f;

    public AudioSource _shootSound = null;
    public AudioSource _emptyGun = null;
    public AudioSource _reloadGun = null;
    public float health { get { return _health; } }
    public float stamina { get { return _fpsController != null ? _fpsController.stamina : 0.0f; } }

    // Private
    private Collider _collider = null;
    private FPSController _fpsController = null;
    private CharacterController _characterController = null;
    private GameSceneManager _gameSceneManager = null;
    private int _aiBodyPartLayer = -1;
    private int _interactiveMask = 0;


    void Start()
    {
        _collider = GetComponent<Collider>();
        _fpsController = GetComponent<FPSController>();
        _characterController = GetComponent<CharacterController>();
        _gameSceneManager = GameSceneManager.instance;

        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");
        _interactiveMask = 1 << LayerMask.NameToLayer("Interactive");

        if (_gameSceneManager != null)
        {
            PlayerInfo info = new PlayerInfo();
            info.camera = _camera;
            info.characterManager = this;
            info.collider = _collider;
            info.meleeTrigger = _meleeTrigger;

            _gameSceneManager.RegisterPlayerInfo(_collider.GetInstanceID(), info);
        }

        //Get rid of mouse cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (_playerHUD) _playerHUD.Fade(2.0f, ScreenFadeType.FadeIn);
    }

    public void TakeDamage(float amount, bool doDamage, bool doPain)
    {
        _health = Mathf.Max(_health - (amount * Time.deltaTime), 0.0f);

        if (_fpsController)
        {
            _fpsController.dragMultiplier = 0.0f;
        }

        if (_cameraBloodEffect != null)
        {
            _cameraBloodEffect.minBloodAmount = (1.0f - _health / 100.0f) * 0.5f;
            _cameraBloodEffect.bloodAmount = Mathf.Min(_cameraBloodEffect.minBloodAmount + 0.3f, 1.0f);
        }

        //Do pain/damage sound
        if (AudioManager.instance)
        {
            if (doDamage && _damageSounds != null)
                AudioManager.instance.PlayOneShotSound(_damageSounds.audioGroup, _damageSounds.audioClip, transform.position, _damageSounds.volume, _damageSounds.spatialBlend, _damageSounds.priority);

            if(doPain && _painSounds != null && _nextPainSoundTime < Time.time)
            {
                AudioClip painClip = _painSounds.audioClip;
                if (painClip)
                {
                    _nextPainSoundTime = Time.time + painClip.length;
                    StartCoroutine(AudioManager.instance.PlayOneShotSoundDelayed(_painSounds.audioGroup, painClip, transform.position, _painSounds.spatialBlend, _painSoundOffset, _painSounds.priority));
                }
            }
        }
    }

    public void DoDamage(int hitDirection = 0)
    {
        if (_camera == null) return;
        if (_gameSceneManager == null) return;

        // Local Variables
        Ray ray;
        RaycastHit hit;
        bool isSomethingHit = false;

        ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        isSomethingHit = Physics.Raycast(ray, out hit, 1000.0f, 1 << _aiBodyPartLayer);

        if (isSomethingHit)
        {
            AIStateMachine stateMachine = _gameSceneManager.GetAIStateMachine(hit.rigidbody.GetInstanceID());
            Debug.Log("Body part is: " + hit.rigidbody.GetInstanceID());
            if (stateMachine)
            {
                Debug.Log("This has worked");
                stateMachine.TakeDamage(hit.point, ray.direction * 1.0f, 50, hit.rigidbody, this, 0);
            }
        }

    }

    void Update()
    {

        Ray ray;
        RaycastHit hit;
        RaycastHit[] hits;

        //Process interactive objects
        //is the crosshair over a useable item or a descriptive item? first get ray from centre of screen
        ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        //calculate ray length
        float rayLength = Mathf.Lerp(1.0f, 1.8f, Mathf.Abs(Vector3.Dot(_camera.transform.forward, Vector3.up)));

        //cast ray and collect all hits
        hits = Physics.RaycastAll(ray, rayLength, _interactiveMask);

        //process the hits for the one with the highest priority
        if (hits.Length > 0)
        {
            //used to record index of the highest priority
            int highestPriority = int.MinValue;
            InteractiveItem priorityObject = null;

            //Iteracte through each hit
            for(int i = 0; i < hits.Length; i++)
            {
                //process next hit
                hit = hits[i];

                //fetch its interactiveitem sscript from the database
                InteractiveItem interactiveObject = _gameSceneManager.GetInteractiveItem(hit.collider.GetInstanceID());

                //if this is the highest priority object so far then remember it
                if(interactiveObject!= null && interactiveObject.priority > highestPriority)
                {
                    priorityObject = interactiveObject;
                    highestPriority = priorityObject.priority;
                }
            }

            //if we found an object then display its text and process any possible activation
            if (priorityObject != null)
            {
                if (_playerHUD)
                    _playerHUD.SetInteractionText(priorityObject.GetText());
                if (Input.GetButtonDown("Use"))
                {
                    priorityObject.Activate(this);
                }
            }
        }
        else
        {
            if (_playerHUD)
                _playerHUD.SetInteractionText(null);
        }

        //are we attacking?!
        if (Input.GetMouseButtonDown(0) && _ammo > 0)
        {
            if (_ammo >= 0)
            {
                _ammo -= 1;
                _shootSound.Play();
                DoDamage();
            }
            //_shootSound.Play();
            //DoDamage();
        }
        if (Input.GetMouseButtonDown(0) && _ammo <= 0)
        {
            _emptyGun.Play();
        }
        if (Input.GetKeyDown("r") && _ammo <= 5 && _ammo >= 0)
        {
            _reloadGun.Play();
            _ammo = _maxAmmo;
            print("Bullets currently: " + _ammo);
        }

        if (_fpsController && _soundEmitter!=null)
        {
            //Bloodradius = zombies can smell player when low health!
            float newRadius = Mathf.Max(_walkRadius, (100.0f - _health)/_bloodRadiusScale);
            switch (_fpsController.movementStatus)
            {
                case PlayerMoveStatus.Landing: newRadius = Mathf.Max(newRadius, _landingRadius); break;
                case PlayerMoveStatus.Running: newRadius = Mathf.Max(newRadius, _runRadius); break;
            }
            _soundEmitter.SetRadius(newRadius);

            _fpsController.dragMultiplierLimit = Mathf.Max(_health / 100.0f, 0.25f);
        }
        Debug.Log("" + _ammo);
        if (_playerHUD) _playerHUD.Invalidate(this);
    }
}

