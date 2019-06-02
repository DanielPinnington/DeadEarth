using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------
// CLASS : AIZombieState_Idle1
// DESC	 : An AIState that implements a zombies Idle Behaviour
// ----------------------------------------------------------------------
public class AIZombieState_Idle1 : AIZombieState
{
    // Inspector Assigned 
    [SerializeField] Vector2 _idleTimeRange = new Vector2(10.0f, 60.0f);

    // Private
    float _idleTime = 0.0f;
    float _timer = 0.0f;

    // ------------------------------------------------------------------
    // Name	:	GetStateType
    // Desc	:	Returns the type of the state
    // ------------------------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }

    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	Called by the State Machine when first transitioned into
    //			this state. It initializes a timer and configures the
    //			the state machine
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Entering Idle State");
        base.OnEnterState();
        if (_zombieStateMachine == null)
            return;

        // Set Idle Time
        _idleTime = Random.Range(_idleTimeRange.x, _idleTimeRange.y);
        _timer = 0.0f;

        // Configure State Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        _zombieStateMachine.ClearTarget();
    }

    // -------------------------------------------------------------------
    // Name	:	OnUpdate
    // Desc	:	Called by the state machine each frame
    // -------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        // No state machine then bail
        if (_zombieStateMachine == null)
            return AIStateType.Idle;

        // Is the player visible
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        // Is the threat a flashlight
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        // Is the threat an audio emitter
        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        // Is the threat food
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        // Update the idle timer
        _timer += Time.deltaTime;

        // Patrol if idle time has been exceeded
        if (_timer > _idleTime)
        {
            _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
            _zombieStateMachine.navAgent.Resume();
            return AIStateType.Alerted;
        }

        // No state change required
        return AIStateType.Idle;
    }
}
