using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootnotion : MonoBehaviour
{
    public AIWaypointNetwork WaypointNetwork = null;
    public int CurrentIndex = 0; //Waypoints
    public bool HasPath = false;
    public bool PathPending = false;
    public bool PathStale = false;
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;

    public AnimationCurve JumpCurve = new AnimationCurve();

    private NavMeshAgent _NavAgent = null;
    private Animator _animator = null;
    private float _originalSpeed = 0;

    // Start is called before the first frame update
    void Start()
    {
        _NavAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        if (_NavAgent) _originalSpeed = _NavAgent.speed;
        if (WaypointNetwork == null) return;
        SetNextDestination(false);
    }

    void SetNextDestination(bool increment)
    {
        // If no network return
        if (!WaypointNetwork) return;

        // Calculatehow much the current waypoint index needs to be incremented
        int incStep = increment ? 1 : 0;

        // Calculate index of next waypoint factoring in the increment with wrap-around and fetch waypoint 
        int nextWaypoint = (CurrentIndex + incStep >= WaypointNetwork.Waypoints.Count) ? 0 : CurrentIndex + incStep;
        Transform nextWaypointTransform = WaypointNetwork.Waypoints[nextWaypoint];

        // Assuming we have a valid waypoint transform
        if (nextWaypointTransform != null)
        {
            // Update the current waypoint index, assign its position as the NavMeshAgents
            // Destination and then return
            CurrentIndex = nextWaypoint;
            _NavAgent.destination = nextWaypointTransform.position;
            return;
        }

        // We did not find a valid waypoint in the list for this iteration
        CurrentIndex++;
    }

    // Update is called once per frame

    void Update()
    {

        int turnOnSpot;
        // Copy NavMeshAgents state into inspector visible variables
        HasPath = _NavAgent.hasPath;
        PathPending = _NavAgent.pathPending;
        PathStale = _NavAgent.isPathStale;
        PathStatus = _NavAgent.pathStatus;

        Vector3 cross = Vector3.Cross(transform.forward, _NavAgent.desiredVelocity.normalized);
        float horizontal = (cross.y < 0) ? -cross.magnitude : cross.magnitude;
        horizontal = Mathf.Clamp(horizontal * 4.32f, -2.32f, 2.32f);

        if (_NavAgent.desiredVelocity.magnitude < 1.0f && Vector3.Angle(transform.forward, _NavAgent.desiredVelocity) > 10.0f)
        {
            _NavAgent.speed = 0.1f;
            turnOnSpot = (int)Mathf.Sign(horizontal);
        }
        else
        {
            _NavAgent.speed = _originalSpeed;
            turnOnSpot = 0;
        }

        _animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        _animator.SetFloat("Vertical", _NavAgent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
        _animator.SetInteger("turnOnSpot", turnOnSpot);
        // If we don't have a path and one isn't pending then set the next
        // waypoint as the target, otherwise if path is stale regenerate path
        if (_NavAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(1.0f)); //Time of the jump in the air
        }
        if ((_NavAgent.remainingDistance <= _NavAgent.stoppingDistance && !PathPending) || PathStatus == NavMeshPathStatus.PathInvalid /*|| PathStatus==NavMeshPathStatus.PathPartial*/)
            SetNextDestination(true);
        else
        if (_NavAgent.isPathStale)
            SetNextDestination(false);

    }
    IEnumerator Jump (float duration)
    {
        OffMeshLinkData data = _NavAgent.currentOffMeshLinkData;
        Vector3 startPos = _NavAgent.transform.position;
        Vector3 endPos = data.endPos + (_NavAgent.baseOffset * Vector3.up);

        float time = 0f;

        while(time <= duration)
        {
            float t = time / duration;
            _NavAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (JumpCurve.Evaluate(t) * Vector3.up);
            time += Time.deltaTime;
            yield return null;
        }
        _NavAgent.CompleteOffMeshLink();
    }
}
