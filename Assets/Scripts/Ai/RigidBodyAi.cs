using UnityEngine;
using UnityEngine.AI;

public class AIRacer3D : MonoBehaviour
{
    public float maxSpeed = 20f;
    public float acceleration = 10f;
    public float turnSpeed = 5f;

    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private Vector3 _desiredVelocity;
    private CarProgress _carProgress;

    private float turnError;
    private float turnIntegral;
    private float turnDerivative;
    private float lastTurnError;

    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        _carProgress = GetComponent<CarProgress>();

        // 🔥 Completely disable NavMesh movement so it doesn't fight Rigidbody
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.isStopped = true; // Prevents pulling effect
    }

    void FixedUpdate()
    {
        // if (_carProgress == null || WaypointManager.Instance == null) return;

        // 🔥 Get the next waypoint position
        Vector3 targetPosition = WaypointManager.Instance.waypoints[_carProgress.currentCheckpoint].position;
        _navMeshAgent.SetDestination(targetPosition);
        _desiredVelocity = _navMeshAgent.desiredVelocity;

        // 🔥 Use NavMesh velocity as movement direction
        Vector3 moveDirection = _desiredVelocity.normalized;

        // Calculate turning error (angle difference)
        float angleError = Vector3.SignedAngle(transform.forward, moveDirection, Vector3.up);

        // PID Controller for smooth turning
        float kp = 0.1f, ki = 0.01f, kd = 0.05f;
        turnIntegral += angleError * Time.fixedDeltaTime;
        turnDerivative = (angleError - lastTurnError) / Time.fixedDeltaTime;
        lastTurnError = angleError;
        float turnForce = (kp * angleError) + (ki * turnIntegral) + (kd * turnDerivative);

        // Apply rotation torque
        _rb.AddTorque(Vector3.up * turnForce, ForceMode.Acceleration);

        // Adjust speed based on turn angle
        float speedFactor = Mathf.Clamp(1f - Mathf.Abs(angleError) / 90f, 0.4f, 1f);
        float targetSpeed = maxSpeed * speedFactor;

        // Apply forward force
        Vector3 moveForce = moveDirection * acceleration * Time.fixedDeltaTime;
        _rb.AddForce(moveForce, ForceMode.Acceleration);
        _rb.linearVelocity = Vector3.ClampMagnitude(_rb.linearVelocity, targetSpeed);

        // Apply lateral friction to prevent oversteering
        Vector3 lateralVelocity = Vector3.Dot(_rb.linearVelocity, transform.right) * transform.right;
        _rb.AddForce(-lateralVelocity * 5f, ForceMode.Acceleration);
    }
}
