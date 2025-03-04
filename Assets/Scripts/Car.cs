using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Car : MonoBehaviour
{
    public float waypointThreshold = 3f;
    public float maxSpeed = 10f;
    public float turnSpeed = 5f;
    public float brakePower = 1f;
    public float Acceleration = 1f;
    public float detectionRange = 5f;
    public float overtakingOffset = 2f;
    public float overtakingSpeedBoost = 2f;
    public float overtakingDuration = 1.5f;
    public float momentumDuration = 2f;
    public float brakingDeceleration = 5f;

    private NavMeshAgent navMeshAgent;
    private WaypointManager waypointManager;
    public int currentWaypointIndex = 0;
    private bool isOvertaking = false;
    private bool raceStarted = false; // Prevent movement before race starts
    private CarProgress carProgress;

    void Start()
    {
        carProgress = GetComponent<CarProgress>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        waypointManager = FindFirstObjectByType<WaypointManager>();

        // Configure NavMeshAgent
        navMeshAgent.speed = 0f; // Keep stationary initially
        navMeshAgent.angularSpeed = 120f;
        navMeshAgent.acceleration = 15f;
        navMeshAgent.autoBraking = false;

        if (waypointManager.waypoints.Count > 0)
        {
            navMeshAgent.SetDestination(waypointManager.waypoints[currentWaypointIndex].position);
        }
        StartRace();
    }

    void Update()
    {
        if (!raceStarted) return; // Prevent movement until race starts

        // Handle waypoint navigation
        if (Vector3.Distance(waypointManager.waypoints[currentWaypointIndex].position, transform.position) < waypointThreshold)
        {
            currentWaypointIndex = currentWaypointIndex >= waypointManager.waypoints.Count - 1 ? 0 : (currentWaypointIndex + 1);

            navMeshAgent.SetDestination(waypointManager.waypoints[currentWaypointIndex].position);

            if (waypointManager.IsFinishLine(currentWaypointIndex))
            {
                HandleLapCompletion();
            }
        }

        // Smooth rotation
        FaceMovementDirection();
        AdjustSpeedForTurns();

        // Handle overtaking

    }

    public void StartRace()
    {
        raceStarted = true;
        navMeshAgent.speed = Mathf.Lerp(navMeshAgent.speed, maxSpeed, Time.deltaTime * Acceleration); // Set speed when race starts
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
        // StartCoroutine(ApplyMomentum());
    }

    private IEnumerator ApplyMomentum()
    {
        float initialSpeed = navMeshAgent.speed;
        Debug.Log($"{name} applying momentum with initial speed: {initialSpeed}");

        // Gradually reduce speed
        float timeElapsed = 0f;
        while (timeElapsed < momentumDuration)
        {
            float remainingSpeed = Mathf.Lerp(initialSpeed, 0f, timeElapsed / momentumDuration);
            navMeshAgent.speed = remainingSpeed;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Fully stop the agent
        navMeshAgent.speed = 0f;
        navMeshAgent.isStopped = true; // Stop pathfinding completely
        Debug.Log($"{name} has fully stopped after finishing the race.");
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
        float targetSpeed = 15 * (1 - Mathf.Abs(turnAngles) / 80f);
        Debug.Log("turn Angle = " + targetSpeed);
        navMeshAgent.acceleration =Mathf.Lerp(5 + Mathf.Pow(maxSpeed, 1.05f), Mathf.Pow(maxSpeed-10, 1.5f) * 70/100, targetSpeed / 15f);

        if (turnAngles > 20f)
        {
            navMeshAgent.speed = Mathf.Lerp(navMeshAgent.speed, targetSpeed, Time.deltaTime * brakePower * 5);
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
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, waypointThreshold); // Draws a wireframe circle
    }
}
