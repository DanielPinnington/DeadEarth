using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AILayeredAudioSourcePlayer : AIStateMachineLink
{
    // Inspector Assigned
    [SerializeField] AudioCollection _collection = null;
    [SerializeField] int _bank = 0;
    [SerializeField] bool _looping = true;
    [SerializeField] bool _stopOnExit = false;

    // Private
    float _prevLayerWeight = 0.0f;

    // --------------------------------------------------------
    // Name	:	OnStateEnter
    // Desc	:	Called prior to the first frame the
    //			animation assigned to this state.
    // --------------------------------------------------------
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (_stateMachine == null) return;

        // Get the layer weight and only play for weighted layer
        float layerWeight = animator.GetLayerWeight(layerIndex);

        if (_collection != null)
        {
            // I used 0.5 weight as the threshold but my layers are either 1 or 0
            if (layerIndex == 0 || layerWeight > 0.5f)
                _stateMachine.PlayAudio(_collection, _bank, layerIndex, _looping);
            else
                _stateMachine.StopAudio(layerIndex);
        }

        // Store layer weight to detect changes mid animation
        _prevLayerWeight = layerWeight;
    }



    // ---------------------------------------------------------
    // Name	:	OnStateUpdated
    // Desc	:	Called by the animation system for each frame  
    //			update of the animation
    // ---------------------------------------------------------
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (_stateMachine == null) return;

        // Get the current layer weight
        float layerWeight = animator.GetLayerWeight(layerIndex);

        // If its changes we might need to start or stop the audio layer assigned to it
        if (layerWeight != _prevLayerWeight && _collection != null)
        {
            if (layerWeight > 0.5f) _stateMachine.PlayAudio(_collection, _bank, layerIndex, true);
            else _stateMachine.StopAudio(layerIndex);
        }

        _prevLayerWeight = layerWeight;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        if (_stateMachine && _stopOnExit)
            _stateMachine.StopAudio(layerIndex);

    }


}
