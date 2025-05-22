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

    public float minAccel = 5f;
    public float maxAccel = 50f;
    public float maxTurnAngle = 80f;

    public int pitBoxIndex;
    public bool pitLap = false;
    private bool pitting = false;

    private NavMeshAgent navMeshAgent;
    private WaypointManager waypointManager;
    private CarProgress carProgress;
    public int currentWaypointIndex = 0;
    private bool raceStarted = false;

    private enum PitState { None, GoingToPit, DrivingToBox, EnterBox, Stopping, ExitBox, ReturningToPitLane, ExitingPit }
    private PitState _pitState = PitState.None;

    private float _pitStopTimer = 0f;
    private float _pitStopDuration = 3f;
    private int _pitExitWaypointIndex = 2;

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

    void Update()
    {
        if (!raceStarted) return;

        pitHandler();

        if (_pitState == PitState.None && Vector3.Distance(waypointManager.waypoints[currentWaypointIndex].position, transform.position) < waypointThreshold)
        {
            currentWaypointIndex = currentWaypointIndex >= waypointManager.waypoints.Count - 1 ? 0 : (currentWaypointIndex + 1);
            if (pitLap && currentWaypointIndex == 0)
            {
                navMeshAgent.SetDestination(waypointManager.pitLaneWaypoint[0].position);
                _pitState = PitState.GoingToPit;
            }
            else
                navMeshAgent.SetDestination(waypointManager.waypoints[currentWaypointIndex].position);


            if (waypointManager.IsFinishLine(currentWaypointIndex))
            {
                HandleLapCompletion();
            }
        }

        FaceMovementDirection();

        if (!pitting)
        {
            AdjustSpeedForTurns();
        }
    }
    public void goPitThisLap()
    {
        pitLap = true;
    }
    float SavedTurnTime;
    public void pitHandler()
    {
        if (!pitLap) return;

        switch (_pitState)
        {
            case PitState.GoingToPit:
                if (Vector3.Distance(transform.position, waypointManager.pitLaneWaypoint[0].position) < 5f)
                {
                    SavedTurnTime = turnSpeed;
                    turnSpeed = 2f;
                    pitting = true;
                    navMeshAgent.velocity = navMeshAgent.desiredVelocity.normalized * 20f;
                    navMeshAgent.speed = 20f;
                    navMeshAgent.acceleration = 1000f;
                    navMeshAgent.SetDestination(waypointManager.GetPitBoxWaypoint(pitBoxIndex,0).position);
                    _pitState = PitState.DrivingToBox;
                }
                break;

            case PitState.DrivingToBox:
                if (Vector3.Distance(transform.position, waypointManager.GetPitBoxWaypoint(pitBoxIndex,0).position) < 2f)
                {
                    _pitState = PitState.EnterBox;
                    navMeshAgent.SetDestination(waypointManager.GetPitBoxWaypoint(pitBoxIndex,1).position);
                }
                break;
            case PitState.EnterBox:
                if(Vector3.Distance(transform.position, waypointManager.GetPitBoxWaypoint(pitBoxIndex,1).position) < 2f)
                {
                    navMeshAgent.SetDestination(waypointManager.GetPitBoxWaypoint(pitBoxIndex,2).position);
                }
                if (Vector3.Distance(transform.position, waypointManager.GetPitBoxWaypoint(pitBoxIndex,2).position) < 2f)
                {
                    navMeshAgent.isStopped = true;
                    _pitStopTimer = 0f;
                    _pitState = PitState.Stopping;
                }
                break;
            case PitState.Stopping:
                _pitStopTimer += Time.deltaTime;
                if (_pitStopTimer >= _pitStopDuration)
                {
                    navMeshAgent.isStopped = false;
                    navMeshAgent.SetDestination(waypointManager.GetPitBoxWaypoint(pitBoxIndex,3).position);
                    _pitState = PitState.ExitBox;
                }
                break;
            case PitState.ExitBox:
                if (Vector3.Distance(transform.position, waypointManager.GetPitBoxWaypoint(pitBoxIndex,3).position) < 1.5f)
                {
                    navMeshAgent.SetDestination(waypointManager.pitLaneWaypoint[1].position);
                    _pitState = PitState.ReturningToPitLane;
                }
                break;
            case PitState.ReturningToPitLane:
                if (Vector3.Distance(transform.position, waypointManager.pitLaneWaypoint[1].position) < 3f)
                {
                    turnSpeed = SavedTurnTime;
                    currentWaypointIndex = 1;
                    navMeshAgent.SetDestination(waypointManager.waypoints[1].position);
                    _pitState = PitState.ExitingPit;
                }
                break;

            case PitState.ExitingPit:
                pitLap = false;
                pitting = false;
                _pitState = PitState.None;
                break;
        }
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

        float angleRatio = Mathf.Clamp01(Mathf.Abs(turnAngles) / maxTurnAngle);
        navMeshAgent.acceleration = Mathf.Lerp(minAccel, maxAccel, angleRatio);

        if (Mathf.Abs(turnAngles) > 40f)
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
