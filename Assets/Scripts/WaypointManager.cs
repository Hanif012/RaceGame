using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [Header("Waypoints")]
    public List<Transform> waypoints = new List<Transform>();
    public List<Transform> pitLaneWaypoint = new List<Transform>();
    public List<Transform> pitBox1 = new List<Transform>();
    public List<Transform> pitBox2 = new List<Transform>();
    public List<Transform> pitBox3 = new List<Transform>();
    public List<Transform> pitBox4 = new List<Transform>();
    public List<Transform> pitBox5 = new List<Transform>();
    public List<Transform> pitBox6 = new List<Transform>();
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
    public Transform GetPitBoxWaypoint(int pitNumber, int index)
    {
        switch(pitNumber)
        {
            case 1:
                return pitBox1[index];
            case 2:
                return pitBox2[index];
            case 3:
                return pitBox3[index];
            case 4:
                return pitBox4[index];
            case 5:
                return pitBox5[index];
            case 6:
                return pitBox6[index];
            default:
                Debug.LogError("WaypointManager: Invalid pit number!");
                return null;
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

            if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(waypoint.position, waypoints[i + 1].position);
            }
            else if (i == waypoints.Count - 1 && waypoints[0] != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(waypoint.position, waypoints[0].position);
            }
        }
        for (int i = 0; i < pitLaneWaypoint.Count; i++)
        {
            Transform waypoint = pitLaneWaypoint[i];
            if (waypoint == null) continue;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(waypoint.position, 1f);
            if (i < pitLaneWaypoint.Count - 1)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(waypoint.position, pitLaneWaypoint[i + 1].position);
            }
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pitLaneWaypoint[pitLaneWaypoint.Count - 1].position, waypoints[1].position);
        Gizmos.DrawLine(pitLaneWaypoint[0].position, waypoints[waypoints.Count - 1].position);
        
        

        for (int i = 0; i < pitBox1.Count; i++)
        {
            Transform waypoint = pitBox1[i];
            if (waypoint == null) continue;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(waypoint.position, 1f);
            if (i < pitBox1.Count - 1)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(waypoint.position, pitBox1[i + 1].position);
            }
        }

        for (int i = 0; i < pitBox2.Count; i++)
        {
            Transform waypoint = pitBox2[i];
            if (waypoint == null) continue;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(waypoint.position, 1f);
            if (i < pitBox2.Count - 1)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(waypoint.position, pitBox2[i + 1].position);
            }
        }
        for (int i = 0; i < pitBox3.Count; i++)
        {
            Transform waypoint = pitBox3[i];
            if (waypoint == null) continue;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(waypoint.position, 1f);
            if (i < pitBox3.Count - 1)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(waypoint.position, pitBox3[i + 1].position);
            }
        }

        for (int i = 0; i < pitBox4.Count; i++)
        {
            Transform waypoint = pitBox4[i];
            if (waypoint == null) continue;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(waypoint.position, 1f);
            if (i < pitBox4.Count - 1)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(waypoint.position, pitBox4[i + 1].position);
            }
        }
        for (int i = 0; i < pitBox5.Count; i++)
        {
            Transform waypoint = pitBox5[i];
            if (waypoint == null) continue;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(waypoint.position, 1f);
            if (i < pitBox5.Count - 1)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(waypoint.position, pitBox5[i + 1].position);
            }
        }
    }
}
