using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Car : MonoBehaviour
{
    public float waypointThreshold = 3f;
    public float maxSpeed = 10f; 
    public float turnSpeed = 5f;
    public float detectionRange = 5f; 
    public float overtakingOffset = 2f; 
    public float overtakingSpeedBoost = 2f; 
    public float overtakingDuration = 1.5f; 
    public float momentumDuration = 2f; 
    public float brakingDeceleration = 5f; 

    private NavMeshAgent navMeshAgent;
    private WaypointManager waypointManager; 
    private int currentWaypointIndex = 0;
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
    }

    void Update()
    {
        if (!raceStarted) return; // Prevent movement until race starts

        // Handle waypoint navigation
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypointManager.waypoints.Count;
            navMeshAgent.SetDestination(waypointManager.waypoints[currentWaypointIndex].position);

            // Check if this waypoint is the finish line
            if (waypointManager.IsFinishLine(currentWaypointIndex))
            {
                HandleLapCompletion();
            }
        }

        // Smooth rotation
        FaceMovementDirection();
        AdjustSpeedForTurns();

        // Handle overtaking
        if (DetectCarAhead() && !isOvertaking)
        {
            StartCoroutine(Overtake());
        }
    }

    public void StartRace()
    {
        raceStarted = true;
        navMeshAgent.speed = maxSpeed; // Set speed when race starts
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
        StartCoroutine(ApplyMomentum());
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

    private bool DetectCarAhead()
    {
        // Detect cars within the detection range in the forward direction
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Car"))
            {
                return true; 
            }
        }
        return false;
    }

    private IEnumerator Overtake()
    {
        isOvertaking = true;

        // Adjust the agent's position to simulate overtaking
        Vector3 overtakingTarget = navMeshAgent.destination + transform.right * overtakingOffset;
        navMeshAgent.SetDestination(overtakingTarget);

        // Apply temporary speed boost
        navMeshAgent.speed = maxSpeed + overtakingSpeedBoost;

        yield return new WaitForSeconds(overtakingDuration);

        // End overtaking behavior
        isOvertaking = false;
        navMeshAgent.speed = maxSpeed;

        // Return to the main waypoint path
        navMeshAgent.SetDestination(waypointManager.waypoints[currentWaypointIndex].position);
    }

    private void AdjustSpeedForTurns()
    {
        Vector3 forward = transform.forward;
        Vector3 waypointDirection = (navMeshAgent.destination - transform.position).normalized;

        float turnAngle = Vector3.Angle(forward, waypointDirection);

        if (turnAngle > 45f)
        {
            navMeshAgent.speed = Mathf.Lerp(navMeshAgent.speed, maxSpeed * 0.5f, Time.deltaTime);
        }
        else
        {
            navMeshAgent.speed = Mathf.Lerp(navMeshAgent.speed, maxSpeed, Time.deltaTime);
        }
    }
    public IEnumerator StartRaceWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartRace();
    }

}
