using UnityEngine;
using System.Collections;
using UnityEngine.AI;

// ----------------------------------------------------------------
// CLASS	:	AIZombieState_Patrol1
// DESC		:	Generic Patrolling Behaviour for a Zombie
// ----------------------------------------------------------------
public class AIZombieState_Patrol1 : AIZombieState
{
    // Inpsector Assigned 
    [SerializeField] AIWaypointNetwork _waypointNetwork = null;
    [SerializeField] bool _randomPatrol = false;
    [SerializeField] int _currentWaypoint = 0;
    [SerializeField] float _turnOnSpotThreshold = 80.0f;
    [SerializeField] float _slerpSpeed = 5.0f;

    [SerializeField] [Range(0.0f, 3.0f)] float _speed = 1.0f;

    // ------------------------------------------------------------
    // Name	:	GetStateType
    // Desc	:	Called by parent State Machine to get this state's
    //			type.
    // ------------------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	Called by the State Machine when first transitioned into
    //			this state. It initializes a timer and configures the
    //			the state machine
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Entering Patrol State");
        base.OnEnterState();
        if (_zombieStateMachine == null)
            return;

        // Configure State Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = _speed;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        // If the current target is not a waypoint then we need to select
        // a waypoint from te waypoint network and make this the new target
        // and plot a path to it
        if (_zombieStateMachine.targetType != AITargetType.Waypoint)
        {
            // Clear any previous target
            _zombieStateMachine.ClearTarget();

            // Do we have a valid waypoint network
            if (_waypointNetwork != null && _waypointNetwork.Waypoints.Count > 0)
            {
                // if this is a random patrol then set current waypoint to a random
                // waypoint index
                if (_randomPatrol)
                {
                    _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);
                }

                // If itd a valid index then fetch the waypoint and make the new target
                if (_currentWaypoint < _waypointNetwork.Waypoints.Count)
                {
                    Transform waypoint = _waypointNetwork.Waypoints[_currentWaypoint];
                    if (waypoint != null)
                    {
                        // This is the new state machines target
                        _zombieStateMachine.SetTarget(AITargetType.Waypoint,
                                                        null,
                                                        waypoint.position,
                                                        Vector3.Distance(_zombieStateMachine.transform.position, waypoint.position)
                                                    );

                        // Tell NavAgent to make a path to this waypoint
                        _zombieStateMachine.navAgent.SetDestination(waypoint.position);

                    }
                }
            }
        }

        // Make sure NavAgent is switched on
        _zombieStateMachine.navAgent.Resume();
    }


    // ------------------------------------------------------------
    // Name	:	OnUpdate
    // Desc	:	Called by the state machine each frame to give this
    //			state a time-slice to update itself. It processes 
    //			threats and handles transitions as well as keeping
    //			the zombie aligned with its proper direction in the
    //			case where root rotation isn't being used.
    // ------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        // Do we have a visual threat that is the player
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        // Sound is the third highest priority
        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        // We have seen a dead body so lets pursue that if we are hungry enough
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            // If the distance to hunger ratio means we are hungry enough to stray off the path that far
            if ((1.0f - _zombieStateMachine.satisfaction) > (_zombieStateMachine.VisualThreat.distance / _zombieStateMachine.sensorRadius))
            {
                _stateMachine.SetTarget(_stateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }
        }

        // Calculate angle we need to turn through to be facing our target
        float angle = Vector3.Angle(_zombieStateMachine.transform.forward, (_zombieStateMachine.navAgent.steeringTarget - _zombieStateMachine.transform.position));

        // If its too big then drop out of Patrol and into Altered
        if (angle > _turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        // If root rotation is not being used then we are responsible for keeping zombie rotated
        // and facing in the right direction. 
        if (!_zombieStateMachine.useRootRotation)
        {
            // Generate a new Quaternion representing the rotation we should have
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);

            // Smoothly rotate to that new rotation over time
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }

        // If for any reason the nav agent has lost its path then call the NextWaypoint function
        // so a new waypoint is selected and a new path assigned to the nav agent.
        if (_zombieStateMachine.navAgent.isPathStale ||
            !_zombieStateMachine.navAgent.hasPath ||
            _zombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            NextWaypoint();
        }


        // Stay in Patrol State
        return AIStateType.Patrol;
    }

    // -------------------------------------------------------------------------
    // Name	:	NextWaypoint
    // Desc	:	Called to select a new waypoint. Either randomly selects a new
    //			waypoint from the waypoint network or increments the current
    //			waypoint index (with wrap-around) to visit the waypoints in
    //			the network in sequence. Sets the new waypoint as the the
    //			target and generates a nav agent path for it
    // -------------------------------------------------------------------------
    private void NextWaypoint()
    {
        // Increase the current waypoint with wrap-around to zero (or choose a random waypoint)
        if (_randomPatrol && _waypointNetwork.Waypoints.Count > 1)
        {
            // Keep generating random waypoint until we find one that isn't the current one
            // NOTE: Very important that waypoint networks do not only have one waypoint :)
            int oldWaypoint = _currentWaypoint;
            while (_currentWaypoint == oldWaypoint)
            {
                _currentWaypoint = Random.Range(0, _waypointNetwork.Waypoints.Count);
            }
        }
        else
            _currentWaypoint = _currentWaypoint == _waypointNetwork.Waypoints.Count - 1 ? 0 : _currentWaypoint + 1;

        // Fetch the new waypoint from the waypoint list
        if (_waypointNetwork.Waypoints[_currentWaypoint] != null)
        {
            Transform newWaypoint = _waypointNetwork.Waypoints[_currentWaypoint];

            // This is our new target position
            _zombieStateMachine.SetTarget(AITargetType.Waypoint,
                                            null,
                                            newWaypoint.position,
                                            Vector3.Distance(newWaypoint.position, _zombieStateMachine.transform.position));

            // Set new Path
            _zombieStateMachine.navAgent.SetDestination(newWaypoint.position);
        }
    }

    // ----------------------------------------------------------------------
    // Name	:	OnDestinationReached
    // Desc	:	Called by the parent StateMachine when the zombie has reached
    //			its target (entered its target trigger
    // ----------------------------------------------------------------------
    public override void OnDestinationReached(bool isReached)
    {
        // Only interesting in processing arricals not departures
        if (_zombieStateMachine == null || !isReached)
            return;

        // Select the next waypoint in the waypoint network
        if (_zombieStateMachine.targetType == AITargetType.Waypoint)
            NextWaypoint();
    }

    // -----------------------------------------------------------------------
    // Name	:	OnAnimatorIKUpdated
    // Desc	:	Override IK Goals
    // -----------------------------------------------------------------------
    public override void OnAnimatorIKUpdated()
    {
        if (_zombieStateMachine == null)
            return;

        _zombieStateMachine.animator.SetLookAtPosition(_zombieStateMachine.targetPosition + Vector3.up);
        _zombieStateMachine.animator.SetLookAtWeight(0.55f);
    }
}
