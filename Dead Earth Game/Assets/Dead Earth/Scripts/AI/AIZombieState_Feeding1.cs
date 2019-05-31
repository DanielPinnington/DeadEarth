using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Feeding1 : AIZombieState
{
    [SerializeField] float _slerpSpeed = 5.0f;

    //Private fields
    private int _eatingStateHash = Animator.StringToHash("Feeding State");
    private int _eatingLayerIndex = - 1;

    //Mandatory Overrides
    public override AIStateType GetStateType() { return AIStateType.Feeding; }
        public override void OnEnterState(){
        Debug.Log("Entering feeding state");
        //Base Class processing
        base.OnEnterState();
        if (_zombieStateMachine == null)
            return;

        //Get layer index
        if (_eatingLayerIndex == -1)
            _eatingLayerIndex = _zombieStateMachine.animator.GetLayerIndex("Cinematic");

        //Configure the state machines animator
        _zombieStateMachine.feeding    = true;
        _zombieStateMachine.seeking    = 0;
        _zombieStateMachine.speed      = 0;
        _zombieStateMachine.attackType = 0;

        //agent updates position but not rotation
        _zombieStateMachine.NavAgentControl(true, false);

    }

    public override void OnExitState()
    {
        _zombieStateMachine.feeding = false;
    }

    public override AIStateType OnUpdate()
    {
        if(_zombieStateMachine.satisfaction > 0.9f)
        {
            _zombieStateMachine.GetWaypointPosition(false); //False stops the incremental waypoint (2 instead of 1 for example) this will carry on with normal waypoint
            return AIStateType.Alerted;
        }

        //If visual threat then drop into alert mode
        if(_zombieStateMachine.VisualThreat.type!=AITargetType.None && _zombieStateMachine.VisualThreat.type != AITargetType.Visual_Food)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        if(_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.animator.GetCurrentAnimatorStateInfo(_eatingLayerIndex).shortNameHash == _eatingStateHash)
        {
            _zombieStateMachine.satisfaction = Mathf.Min(_zombieStateMachine.satisfaction + ((Time.deltaTime * _zombieStateMachine.replenishRate) / 100), 1.0f);
        }

        if (!_zombieStateMachine.useRootRotation)
        {
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = _zombieStateMachine.transform.position.y;
            Quaternion newRot = Quaternion.LookRotation(targetPos - _zombieStateMachine.transform.position);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }

        //Stay in feeding state
        return AIStateType.Feeding;
    }
}
