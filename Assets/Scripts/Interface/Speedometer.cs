using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using LitMotion.Extensions;

public class Speedometer : MonoBehaviour
{
    [Header("Speed Sources")]
    [Tooltip("The car that the speedometer will track")]
    [SerializeField] private GameObject targetVehicle;
    
    [SerializeField] private float maxSpeed = 300f; // Maximum speed in KPH
    [SerializeField] private float speedMultiplier = 3.6f; // Multiplier to convert m/s to KPH
    
    [Header("UI Components")]
    [Tooltip("Text component that displays the speed value")]
    [SerializeField] private TextMeshProUGUI speedText;
    
    [Tooltip("Image that rotates to display current speed")]
    [SerializeField] private RectTransform needleImage;
    
    [Header("Needle Settings")]
    [SerializeField] private float minRotation = -120f; // Minimum needle rotation (0 speed)
    [SerializeField] private float maxRotation = 120f; // Maximum needle rotation (max speed)
    [SerializeField] private float needleSmoothing = 5f; // How smooth the needle movement is
    
    // Private variables
    private Rigidbody vehicleRigidbody;
    private float currentSpeed;
    private float targetNeedleRotation;
    private float currentNeedleRotation;

    private void Start()
    {
        // Find the vehicle's Rigidbody component if one isn't assigned
        if (targetVehicle == null)
        {
            targetVehicle = GameObject.FindWithTag("Player");
            if (targetVehicle == null)
            {
                Debug.LogError("No target vehicle assigned to speedometer!");
                return;
            }
        }
        
        // Get or find the Rigidbody component
        vehicleRigidbody = targetVehicle.GetComponent<Rigidbody>();
        if (vehicleRigidbody == null)
        {
            // Try finding Rigidbody in children if not on the main object
            vehicleRigidbody = targetVehicle.GetComponentInChildren<Rigidbody>();
            if (vehicleRigidbody == null)
            {
                Debug.LogWarning("No Rigidbody found on target vehicle. Using Transform-based speed calculation instead.");
            }
        }
        
        // Set initial needle rotation
        if (needleImage != null)
        {
            currentNeedleRotation = minRotation;
            needleImage.localEulerAngles = new Vector3(0, 0, currentNeedleRotation);
        }
    }

    private void Update()
    {
        UpdateSpeedReading();
        UpdateSpeedometer();
    }

    private void UpdateSpeedReading()
    {
        if (targetVehicle == null) return;
        
        // Calculate speed based on the vehicle's velocity
        if (vehicleRigidbody != null)
        {
            currentSpeed = vehicleRigidbody.linearVelocity.magnitude * speedMultiplier;
        }
        else
        {
            // Fallback to calculating speed using Transform if no Rigidbody is available
            Vector3 lastPosition = targetVehicle.transform.position;
            currentSpeed = (targetVehicle.transform.position - lastPosition).magnitude / Time.deltaTime * speedMultiplier;
        }
        
        // Clamp the speed to prevent exceeding maxSpeed
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
    }

    private void UpdateSpeedometer()
    {
        // Update the speed text
        if (speedText != null)
        {
            speedText.text = Mathf.RoundToInt(currentSpeed).ToString();
        }
        
        // Update the needle rotation
        if (needleImage != null)
        {
            // Calculate the target rotation based on current speed
            targetNeedleRotation = Mathf.Lerp(minRotation, maxRotation, currentSpeed / maxSpeed);
            
            // Smoothly rotate the needle towards the target rotation
            currentNeedleRotation = Mathf.Lerp(currentNeedleRotation, targetNeedleRotation, Time.deltaTime * needleSmoothing);
            
            // Apply the rotation to the needle
            needleImage.localEulerAngles = new Vector3(0, 0, currentNeedleRotation);
        }
    }

    // Public method to manually set the target vehicle
    public void SetTargetVehicle(GameObject vehicle)
    {
        targetVehicle = vehicle;
        
        if (targetVehicle != null)
        {
            vehicleRigidbody = targetVehicle.GetComponent<Rigidbody>();
            if (vehicleRigidbody == null)
            {
                vehicleRigidbody = targetVehicle.GetComponentInChildren<Rigidbody>();
            }
        }
    }
}
