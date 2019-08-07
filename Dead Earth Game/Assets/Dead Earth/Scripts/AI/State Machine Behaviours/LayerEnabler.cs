using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerEnabler : AIStateMachineLink
{
    public bool OnEnter = false;
    public bool OnExit = false;

    // --------------------------------------------------------
    // Name	:	OnStateEnter
    // Desc	:	Called prior to the first frame the
    //			animation assigned to this state.
    // --------------------------------------------------------
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (_stateMachine)
            _stateMachine.SetLayerActive(animator.GetLayerName(layerIndex), OnEnter);
    }

    // --------------------------------------------------------
    // Name	:	OnStateExit
    // Desc	:	Called on the last frame of the animation prior
    //			to leaving the state.
    // --------------------------------------------------------
    override public void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (_stateMachine)
            _stateMachine.SetLayerActive(animator.GetLayerName(layerIndex), OnExit);
    }

}
