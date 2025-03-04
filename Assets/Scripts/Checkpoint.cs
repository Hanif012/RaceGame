// using System.Diagnostics;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex; // Assign this in the Inspector

    public bool isFinishLine = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something COlllider");
        CarProgress car = other.GetComponent<CarProgress>();
        Debug.Log(car.name + "is at checkpoint" + checkpointIndex);
        if (car != null)
        {
            Debug.Log(car.name + "is at checkpoint" + checkpointIndex);
            car.UpdateCheckpoint(checkpointIndex);
        }
    }
}
