using UnityEngine;
using System.Collections;

// ------------------------------------------------------------
// CLASS	:	RootMotionConfigurator
// DESC		:	A State Machine Behaviour that communicates
//				with an AIStateMachine derived class to
//				allow for enabling/disabling root motion on
//				a per animation state basis.
// ------------------------------------------------------------
public class RootMotionConfigurator : AIStateMachineLink
{
    // Inspector Assigned Reference Incrementing Variables
    [SerializeField] private int _rootPosition = 0;
    [SerializeField] private int _rootRotation = 0;

    private bool _rootMotionProcessed = false;

    // --------------------------------------------------------
    // Name	:	OnStateEnter
    // Desc	:	Called prior to the first frame the
    //			animation assigned to this state.
    // --------------------------------------------------------
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        // Request the enabling/disabling of root motion for this animation state 
        if (_stateMachine)
        {
            _stateMachine.AddRootMotionRequest(_rootPosition, _rootRotation);
            _rootMotionProcessed = true;
        }

    }

    // --------------------------------------------------------
    // Name	:	OnStateExit
    // Desc	:	Called on the last frame of the animation prior
    //			to leaving the state.
    // --------------------------------------------------------
    override public void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        // Inform the AI State Machine that we wish to relinquish our root motion request.
        if (_stateMachine && _rootMotionProcessed)
        {
            _stateMachine.AddRootMotionRequest(-_rootPosition, -_rootRotation);
            _rootMotionProcessed = false;
        }
    }
}
