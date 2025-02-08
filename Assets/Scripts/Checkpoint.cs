using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex; // Assign this in the Inspector

    public bool isFinishLine = false;

    private void OnTriggerEnter(Collider other)
    {
        CarProgress car = other.GetComponent<CarProgress>();
        if (car != null)
        {
            car.UpdateCheckpoint(checkpointIndex);
        }
    }
}
