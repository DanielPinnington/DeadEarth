using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDamageTrigger : MonoBehaviour
{
    [SerializeField] string _paramater = "";
    [SerializeField] int _bloodParticlesBurstAmount = 10;

    //Private
    AIStateMachine _stateMachine = null;
    Animator _animator = null;
    int _parameterHash = -1;

    private void Start()
    {
        _stateMachine = transform.root.GetComponentInChildren<AIStateMachine>();
        if (_stateMachine != null)
            _animator = _stateMachine.animator;
            _parameterHash = Animator.StringToHash(_paramater);
    }
    void OnTriggerStay(Collider col)
    {
        if (!_animator)
            return;

        if (col.gameObject.CompareTag("Player") && _animator.GetFloat(_parameterHash) > 0.9f)
        {
            if(GameSceneManager.instance && GameSceneManager.instance.bloodParticles)
            {
                ParticleSystem system = GameSceneManager.instance.bloodParticles;

                //Temporary code
                system.transform.position = transform.position;
                system.transform.rotation = Camera.main.transform.rotation;

                system.simulationSpace = ParticleSystemSimulationSpace.World;
                system.Emit(_bloodParticlesBurstAmount);
            }
            Debug.Log("Player being damaged");
        }
    }
}
