using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [Header("Waypoints")]
    public List<Transform> waypoints = new List<Transform>();
    public Transform pitLane;

    private int finishLineIndex = -1; // Index of the finish line waypoint
    public int TotalWaypoints => waypoints.Count;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ValidateWaypoints();
    }

    public Transform GetNextWaypoint(int currentWaypointIndex)
    {
        if (waypoints.Count == 0)
        {
            Debug.LogError("WaypointManager: No waypoints configured!");
            return null;
        }
        return waypoints[(currentWaypointIndex + 1) % waypoints.Count];
    }

    public bool IsFinishLine(int currentIndex)
    {
        return currentIndex == finishLineIndex;
    }

    private void ValidateWaypoints()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("WaypointManager: No waypoints configured! Add waypoints in the Inspector.");
            return;
        }

        // Find the waypoint marked as the finish line
        finishLineIndex = -1;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Checkpoint checkpoint = waypoints[i].GetComponent<Checkpoint>();
            if (checkpoint != null && checkpoint.isFinishLine)
            {
                finishLineIndex = i;
                Debug.Log($"WaypointManager: Finish line set to '{waypoints[i].name}' (Index {finishLineIndex}).");
                break;
            }
        }

        if (finishLineIndex == -1)
        {
            Debug.LogError("WaypointManager: No waypoint is marked as the finish line! Please assign one in the Checkpoint script.");
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Transform waypoint = waypoints[i];
            if (waypoint == null) continue;

            Checkpoint checkpoint = waypoint.GetComponent<Checkpoint>();
            if (checkpoint != null && checkpoint.isFinishLine)
            {
                Gizmos.color = Color.green; // Highlight finish line
            }
            else
            {
                Gizmos.color = Color.red; // Other waypoints
            }

            Gizmos.DrawSphere(waypoint.position, 1f);

            // Draw lines to the next waypoint for better visualization
            if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(waypoint.position, waypoints[i + 1].position);
            }
            else if (i == waypoints.Count - 1 && waypoints[0] != null)
            {
                // Loop back to the first waypoint
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(waypoint.position, waypoints[0].position);
            }
        }
    }
}
