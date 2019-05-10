using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; //Editor Script
using UnityEngine.AI;

[CustomEditor(typeof(AIWaypointNetwork))] //CustomEditor of type AIWaypointNetwork

public class NewBehaviourScript : Editor //Editor Script
{
    public override void OnInspectorGUI()
    {
        AIWaypointNetwork network = (AIWaypointNetwork)target;
        network.DisplayMode = (PathDisplayMode)EditorGUILayout.EnumPopup("Display Mode", network.DisplayMode);
        if (network.DisplayMode == PathDisplayMode.Paths)
        {
            network.UIStart = EditorGUILayout.IntSlider("Waypoint Start", network.UIStart, 0, network.Waypoints.Count - 1);
            network.UIEnd = EditorGUILayout.IntSlider("Waypoint End", network.UIEnd, 0, network.Waypoints.Count - 1);
        }
        DrawDefaultInspector();
    }
    private void OnSceneGUI()
    {
        AIWaypointNetwork network = (AIWaypointNetwork)target;

        for (int i = 0; i < network.Waypoints.Count; i++) // For every Waypoint in the scene
            {
                if (network.Waypoints[i] != null) //If it isn't null do this
                    Handles.Label(network.Waypoints[i].position, "Waypoints " + i.ToString()); //Label around its position it's number
            }


        if (network.DisplayMode == PathDisplayMode.Connections)
        {
            Vector3[] linePoints = new Vector3[network.Waypoints.Count + 1];

            for (int i = 0; i <= network.Waypoints.Count; i++)

            {
                int index = i != network.Waypoints.Count ? i : 0;
                if (network.Waypoints[index] != null)
                    linePoints[i] = network.Waypoints[index].position;
                else
                    linePoints[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }
            Handles.color = Color.cyan;
            Handles.DrawPolyLine(linePoints);
        }
        else
        {
            if (network.DisplayMode == PathDisplayMode.Paths)
            {
                NavMeshPath path = new NavMeshPath();

                if (network.Waypoints[network.UIStart] != null && network.Waypoints[network.UIEnd] != null)
                {
                    Vector3 from = network.Waypoints[network.UIStart].position;
                    Vector3 to = network.Waypoints[network.UIEnd].position;

                    NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
                    Handles.color = Color.green;
                    Handles.DrawPolyLine(path.corners);
                }
            }
        }
    }
}
