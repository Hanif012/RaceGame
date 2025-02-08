using System.Collections.Generic;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance { get; private set; }
    public List<CarProgress> cars = new();
    public GameObject FirstCar { get; private set; }

    private bool isFirstCarAssigned = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        cars.Sort((a, b) =>
        {
            if (a.completedLaps != b.completedLaps)
                return b.completedLaps.CompareTo(a.completedLaps); 
            if (a.currentCheckpoint != b.currentCheckpoint)
                return b.currentCheckpoint.CompareTo(a.currentCheckpoint); 
            return a.distanceToNextCheckpoint.CompareTo(b.distanceToNextCheckpoint); 
        });

        if (!isFirstCarAssigned)
        {
            foreach (var car in cars)
            {
                if (car.isFinished)
                {
                    FirstCar = car.gameObject;
                    isFirstCarAssigned = true;
                    Debug.Log($"First car to finish: {FirstCar.name}");
                    break;
                }
            }
        }
    }
}
