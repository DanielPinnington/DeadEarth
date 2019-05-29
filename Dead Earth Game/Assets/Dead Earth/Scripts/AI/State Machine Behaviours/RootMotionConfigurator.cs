using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RootMotionConfigurator : AIStateMachineLink
{
    [SerializeField] private int _rootPosition = 0;
    [SerializeField] private int _rootRotation = 0;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_stateMachine)
        {
            Debug.Log(_stateMachine.ToString());
            _stateMachine.AddRootMotionRequest(_rootPosition, _rootRotation);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_stateMachine)
            _stateMachine.AddRootMotionRequest(-_rootPosition, -_rootRotation);
    }

}
