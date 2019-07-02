using UnityEngine;
using System.Collections;

public class AIDamageTrigger : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] string _parameter = "";
    [SerializeField] int _bloodParticlesBurstAmount = 10;
    [SerializeField] float _damageAmount = 0.1f;

    // Private Variables
    AIStateMachine _stateMachine = null;
    Animator _animator = null;
    int _parameterHash = -1;
    GameSceneManager _gameSceneManager = null;

    // ------------------------------------------------------------
    // Name	:	Start
    // Desc	:	Called on object start-up to initialize the script.
    // ------------------------------------------------------------
    void Start()
    {
        // Cache state machine and animator references
        _stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();

        if (_stateMachine != null)
            _animator = _stateMachine.animator;

        // Generate parameter hash for more efficient parameter lookups from the animator
        _parameterHash = Animator.StringToHash(_parameter);

        _gameSceneManager = GameSceneManager.instance;
    }

    // -------------------------------------------------------------
    // Name	:	OnTriggerStay
    // Desc	:	Called by Unity each fixed update that THIS trigger
    //			is in contact with another.
    // -------------------------------------------------------------
    void OnTriggerStay(Collider col)
    {
        // If we don't have an animator return
        if (!_animator)
            return;

        // If this is the player object and our parameter is set for damage
        if (col.gameObject.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f)
        {
            if (GameSceneManager.instance && GameSceneManager.instance.bloodParticles)
            {
                ParticleSystem system = GameSceneManager.instance.bloodParticles;

                // Temporary Code
                system.transform.position = transform.position;
                system.transform.rotation = Camera.main.transform.rotation;

                system.simulationSpace = ParticleSystemSimulationSpace.World;
                system.Emit(_bloodParticlesBurstAmount);
            }

            if (_gameSceneManager != null)
            {
                PlayerInfo info = _gameSceneManager.GetPlayerInfo(col.GetInstanceID());
                if (info != null && info.characterManager != null)
                {
                    info.characterManager.TakeDamage(_damageAmount);
                }
            }
        }
    }
}
