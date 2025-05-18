using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;

public class Car : MonoBehaviour
{
    public float waypointThreshold = 3f;
    public float maxSpeed = 10f;
    public float turnSpeed = 5f;
    public float brakePower = 1f;
    public float Acceleration = 1f;
    public float detectionRange = 5f;

    private NavMeshAgent navMeshAgent;
    private WaypointManager waypointManager;
    public int currentWaypointIndex = 0;
    private bool raceStarted = false;
    private CarProgress carProgress;

    // Acceleration tuning
    public float minAccel = 5f;
    public float maxAccel = 50f;
    public float maxTurnAngle = 80f;
    public bool pitLap = false;
    public bool pitting = false;

    void Start()
    {
        carProgress = GetComponent<CarProgress>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        waypointManager = FindFirstObjectByType<WaypointManager>();

        navMeshAgent.speed = 0f;
        navMeshAgent.angularSpeed = 120f;
        navMeshAgent.acceleration = 15f;
        navMeshAgent.autoBraking = false;

        if (waypointManager.waypoints.Count > 0)
        {
            navMeshAgent.SetDestination(waypointManager.waypoints[currentWaypointIndex].position);
        }

        StartRace();
    }
    public void PitThisLap()
    {
        pitLap = true;
    }
    void Pitting()
    {
        navMeshAgent.SetDestination(waypointManager.waypoints[1].position);
        currentWaypointIndex = 1;
        return;
    }
    void Update()
    {
        if (!raceStarted) return;
        if(pitLap && Vector3.Distance(waypointManager.pitLane.position, transform.position) < waypointThreshold)
        {
            pitLap = false;
            Invoke("Pitting",2f);
        }
        if (Vector3.Distance(waypointManager.waypoints[currentWaypointIndex].position, transform.position) < waypointThreshold)
        {
            currentWaypointIndex = currentWaypointIndex >= waypointManager.waypoints.Count - 1 ? 0 : (currentWaypointIndex + 1);
            if(pitLap && currentWaypointIndex == 15)
            {
                pitting = true;
                navMeshAgent.speed = 10f;
                navMeshAgent.acceleration = 25f;
                navMeshAgent.SetDestination(waypointManager.pitLane.position);
            }
            else
            {
                if(pitting)
                    pitting = false;
                navMeshAgent.SetDestination(waypointManager.waypoints[currentWaypointIndex].position);
            }

            if (waypointManager.IsFinishLine(currentWaypointIndex))
            {
                HandleLapCompletion();
            }
        }

        FaceMovementDirection();
        if(!pitting)
            AdjustSpeedForTurns();
    }

    public void StartRace()
    {
        raceStarted = true;
        Debug.Log($"{name} has started the race!");
    }

    private void HandleLapCompletion()
    {
        if (waypointManager.IsFinishLine(currentWaypointIndex) && carProgress.isFinished)
        {
            FinishRace();
        }
    }

    private void FinishRace()
    {
        Debug.Log($"{name} has finished the race. Starting momentum...");
    }

    private void FaceMovementDirection()
    {
        if (navMeshAgent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direction = navMeshAgent.velocity.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
    }

    private void AdjustSpeedForTurns()
    {
        Vector3 desiredDirection = navMeshAgent.desiredVelocity.normalized;
        float turnAngles = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
        float targetSpeed = 15f * (1f - Mathf.Abs(turnAngles) / maxTurnAngle);

        // Acceleration increases with turn angle
        float angleRatio = Mathf.Clamp01(Mathf.Abs(turnAngles) / maxTurnAngle);
        navMeshAgent.acceleration = Mathf.Lerp(minAccel, maxAccel, angleRatio);

        if (Mathf.Abs(turnAngles) > 10f)
        {
            navMeshAgent.speed = Mathf.Lerp(navMeshAgent.speed, targetSpeed, Time.deltaTime * brakePower * 5f);
        }
        else
        {
            navMeshAgent.speed = Mathf.Lerp(navMeshAgent.speed, maxSpeed, Time.deltaTime * Acceleration);
        }
    }

    public IEnumerator StartRaceWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartRace();
    }

    void OnDrawGizmos()
    {
        if (navMeshAgent != null)
        {
            Vector3 desiredDirection = navMeshAgent.desiredVelocity.normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + desiredDirection * 5f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, waypointThreshold);
    }
}
