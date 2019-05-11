using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour
{
    public AIWaypointNetwork WaypointNetwork = null;
    public int CurrentIndex = 0; //Waypoints
    public bool HasPath = false;
    public bool PathPending = false;
    public bool PathStale = false;
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;

    public AnimationCurve JumpCurve = new AnimationCurve();

    private NavMeshAgent _NavAgent = null;

    // Start is called before the first frame update
    void Start()
    {
        _NavAgent = GetComponent<NavMeshAgent>();
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
        // Copy NavMeshAgents state into inspector visible variables
        HasPath = _NavAgent.hasPath;
        PathPending = _NavAgent.pathPending;
        PathStale = _NavAgent.isPathStale;
        PathStatus = _NavAgent.pathStatus;

        // If we don't have a path and one isn't pending then set the next
        // waypoint as the target, otherwise if path is stale regenerate path
        if (_NavAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(2.0f));
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
            _NavAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + JumpCurve.Evaluate(t) * Vector3.up;
            time += Time.deltaTime;
            yield return null;
        }
        _NavAgent.CompleteOffMeshLink();
    }
}
