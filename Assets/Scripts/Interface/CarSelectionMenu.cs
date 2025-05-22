using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Car selection menu controller that uses the NFS_Carousel component
/// to create a Need For Speed: Most Wanted style car selection screen
/// </summary>
public class CarSelectionMenu : MonoBehaviour
{
    [System.Serializable]
    public class CarSelectedEvent : UnityEvent<string> { }

    [Header("UI References")]
    [SerializeField] private NFS_Carousel carCarousel;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI carNameText;
    [SerializeField] private TextMeshProUGUI carStatsText;
    
    [Header("Car Stats Panel")]
    [SerializeField] private Image topSpeedBar;
    [SerializeField] private Image accelerationBar;
    [SerializeField] private Image handlingBar;
    [SerializeField] private Image brakeBar;
    
    [Header("Car Images")]
    [SerializeField] private Image carPreviewImage;
    [SerializeField] private List<CarData> availableCars = new List<CarData>();
    
    [Header("Car Selection Effects")]
    [SerializeField] private ParticleSystem selectionParticles;
    [SerializeField] private AudioClip selectionSound;
    [SerializeField] private AudioClip navigationSound;
    [SerializeField] private AudioClip backSound;
    
    [Header("Events")]
    public CarSelectedEvent onCarSelected;
    public UnityEvent onBackPressed;

    private AudioSource audioSource;

    // Data structure to store car information
    [System.Serializable]
    public class CarData
    {
        public string carId;
        public string carName;
        public string carDescription;
        public Sprite carIcon;
        public Sprite carPreviewImage;
        [Range(0, 1)] public float topSpeed = 0.5f;
        [Range(0, 1)] public float acceleration = 0.5f;
        [Range(0, 1)] public float handling = 0.5f;
        [Range(0, 1)] public float braking = 0.5f;
        public CarouselItem.ItemType unlockState = CarouselItem.ItemType.Unlocked;
        public Color carColor = Color.white;
    }

    private void Awake()
    {
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Set up button listeners
        if (selectButton != null)
            selectButton.onClick.AddListener(SelectCurrentCar);
            
        if (backButton != null)
            backButton.onClick.AddListener(BackPressed);
    }

    private void OnEnable()
    {
        // Subscribe to carousel selection change event
        if (carCarousel != null)
            carCarousel.onSelectionChanged.AddListener(OnCarouselSelectionChanged);
    }

    private void OnDisable()
    {
        // Unsubscribe from carousel events
        if (carCarousel != null)
            carCarousel.onSelectionChanged.RemoveListener(OnCarouselSelectionChanged);
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (selectButton != null)
            selectButton.onClick.RemoveListener(SelectCurrentCar);
            
        if (backButton != null)
            backButton.onClick.RemoveListener(BackPressed);
    }

    private void Start()
    {
        // Initialize the car selection
        PopulateCarousel();
        UpdateCarDisplay(0);
    }

    /// <summary>
    /// Populate the carousel with available cars
    /// </summary>
    private void PopulateCarousel()
    {
        if (carCarousel == null || availableCars.Count == 0)
            return;
            
        // Clear existing items
        CarouselItem[] existingItems = carCarousel.GetComponentsInChildren<CarouselItem>();
        foreach (var item in existingItems)
        {
            Destroy(item.gameObject);
        }
        
        // Add all car data as carousel items
        foreach (var car in availableCars)
        {
            CarouselItem newItem = carCarousel.AddItem(car.carName, car.carDescription, car.carIcon, car.unlockState);
            if (newItem != null)
            {
                newItem.ItemId = car.carId;
                newItem.ItemColor = car.carColor;
                
                // Subscribe to item clicked event
                newItem.onItemClicked.AddListener(OnCarItemClicked);
            }
        }
    }

    /// <summary>
    /// Handle when a car item is directly clicked on the carousel
    /// </summary>
    private void OnCarItemClicked(CarouselItem item)
    {
        // Find the index of this item in the carousel
        CarouselItem[] allItems = carCarousel.GetComponentsInChildren<CarouselItem>();
        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] == item)
            {
                // Go to this item
                carCarousel.GoToItem(i);
                break;
            }
        }
    }

    /// <summary>
    /// Handle selection change in the carousel
    /// </summary>
    private void OnCarouselSelectionChanged(int index)
    {
        // Play navigation sound
        PlaySound(navigationSound);
        
        // Update the car display with stats
        UpdateCarDisplay(index);
    }

    /// <summary>
    /// Update the UI with the selected car's information
    /// </summary>
    private void UpdateCarDisplay(int index)
    {
        if (index < 0 || index >= availableCars.Count)
            return;
            
        CarData selectedCar = availableCars[index];
        
        // Update car name
        if (carNameText != null)
            carNameText.text = selectedCar.carName;
            
        // Update stats text
        if (carStatsText != null)
            carStatsText.text = selectedCar.carDescription;
            
        // Update car preview image
        if (carPreviewImage != null && selectedCar.carPreviewImage != null)
            carPreviewImage.sprite = selectedCar.carPreviewImage;
            
        // Update stat bars
        UpdateStatBars(selectedCar);
        
        // Update select button state
        if (selectButton != null)
            selectButton.interactable = selectedCar.unlockState != CarouselItem.ItemType.Locked;
    }

    /// <summary>
    /// Update the visual stat bars for the selected car
    /// </summary>
    private void UpdateStatBars(CarData car)
    {
        // Update stat bars with smooth animations
        if (topSpeedBar != null)
            AnimateStatBar(topSpeedBar, car.topSpeed);
            
        if (accelerationBar != null)
            AnimateStatBar(accelerationBar, car.acceleration);
            
        if (handlingBar != null)
            AnimateStatBar(handlingBar, car.handling);
            
        if (brakeBar != null)
            AnimateStatBar(brakeBar, car.braking);
    }

    /// <summary>
    /// Animate a stat bar to a target fill amount
    /// </summary>
    private void AnimateStatBar(Image bar, float targetFill)
    {
        // Animate the bar fill
        LitMotion.LMotion.Create(bar.fillAmount, targetFill, 0.5f)
            .WithEase(LitMotion.Ease.OutQuart)
            .WithOnComplete(() => bar.fillAmount = targetFill);
    }

    /// <summary>
    /// Select the current car and trigger the selection event
    /// </summary>
    public void SelectCurrentCar()
    {
        int currentIndex = carCarousel.GetCurrentIndex();
        if (currentIndex >= 0 && currentIndex < availableCars.Count)
        {
            CarData selectedCar = availableCars[currentIndex];
            
            // Don't allow selecting locked cars
            if (selectedCar.unlockState == CarouselItem.ItemType.Locked)
                return;
                
            // Play selection effects
            PlaySelectionEffects();
            
            // Trigger the car selected event
            onCarSelected?.Invoke(selectedCar.carId);
        }
    }

    /// <summary>
    /// Play visual and audio effects when a car is selected
    /// </summary>
    private void PlaySelectionEffects()
    {
        // Play particle effect
        if (selectionParticles != null)
            selectionParticles.Play();
            
        // Play selection sound
        PlaySound(selectionSound);
        
        // Get the current car item and play its animation
        CarouselItem selectedItem = carCarousel.GetSelectedItem();
        if (selectedItem != null)
            selectedItem.PlaySelectAnimation();
    }

    /// <summary>
    /// Handle back button press
    /// </summary>
    private void BackPressed()
    {
        // Play back sound
        PlaySound(backSound);
        
        // Trigger back event
        onBackPressed?.Invoke();
    }
    
    /// <summary>
    /// Play a sound effect
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
