using UnityEngine;

public class CarProgress : MonoBehaviour
{
    public int currentCheckpoint = 0;
    public int completedLaps = 0;
    public int totalLaps = 3; // Total laps for the race
    public bool isFinished = false; // Flag for race completion
    public float distanceToNextCheckpoint = 0f;

    private Transform nextCheckpoint;

    private void Update()
    {
        if (nextCheckpoint != null)
        {
            // Calculate the distance to the next checkpoint
            distanceToNextCheckpoint = Vector3.Distance(transform.position, nextCheckpoint.position);
        }
    }
    public void UpdateCheckpoint(int newCheckpointIndex)
    {
        // Debug.Log($"Current Checkpoint: {currentCheckpoint}, New Checkpoint: {newCheckpointIndex}, Completed Laps: {completedLaps}");

        // Check if the car is transitioning from the last checkpoint to the finish line
        if (newCheckpointIndex == 0 && currentCheckpoint == WaypointManager.Instance.TotalWaypoints - 1)
        {
            completedLaps++;
            // Debug.Log($"{name} completed lap {completedLaps}/{totalLaps}");

            // Only set isFinished if total laps are completed
            if (completedLaps >= totalLaps)
            {
                Debug.Log($"{name} has finished the race!");
                isFinished = true;
            }
        }

        currentCheckpoint = newCheckpointIndex;
        nextCheckpoint = WaypointManager.Instance.GetNextWaypoint(newCheckpointIndex);
    }


}
