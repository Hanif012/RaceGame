using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Example script that demonstrates how to set up and use the NFS_Carousel.
/// Add this to a scene with the carousel to test it.
/// </summary>
public class NFS_CarouselExample : MonoBehaviour
{
    [SerializeField] private NFS_Carousel carousel;
    [SerializeField] private GameObject carItemPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private TextMeshProUGUI selectionText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image previewImage;
    
    [SerializeField] private List<CarSelectionMenu.CarData> exampleCars = new List<CarSelectionMenu.CarData>();

    // Example car data for testing
    private void CreateExampleCars()
    {
        if (exampleCars.Count > 0) return;
        
        // Create some example cars if none are defined
        exampleCars.Add(new CarSelectionMenu.CarData
        {
            carId = "bmw_m3_gtr", 
            carName = "BMW M3 GTR", 
            carDescription = "The iconic hero car from Most Wanted",
            topSpeed = 0.85f,
            acceleration = 0.9f,
            handling = 0.75f,
            braking = 0.8f,
            unlockState = CarouselItem.ItemType.Unlocked
        });
        
        exampleCars.Add(new CarSelectionMenu.CarData
        {
            carId = "mazda_rx8", 
            carName = "Mazda RX-8", 
            carDescription = "Starter car with balanced performance",
            topSpeed = 0.6f,
            acceleration = 0.65f,
            handling = 0.7f,
            braking = 0.6f,
            unlockState = CarouselItem.ItemType.Unlocked
        });
        
        exampleCars.Add(new CarSelectionMenu.CarData
        {
            carId = "lamborghini_gallardo", 
            carName = "Lamborghini Gallardo", 
            carDescription = "High-end exotic with extreme speed",
            topSpeed = 0.95f,
            acceleration = 0.9f,
            handling = 0.65f,
            braking = 0.7f,
            unlockState = CarouselItem.ItemType.Locked
        });
        
        exampleCars.Add(new CarSelectionMenu.CarData
        {
            carId = "ford_mustang", 
            carName = "Ford Mustang GT", 
            carDescription = "American muscle with raw power",
            topSpeed = 0.8f,
            acceleration = 0.7f,
            handling = 0.6f,
            braking = 0.6f,
            unlockState = CarouselItem.ItemType.Unlocked
        });
        
        exampleCars.Add(new CarSelectionMenu.CarData
        {
            carId = "audi_tt", 
            carName = "Audi TT", 
            carDescription = "German engineering with precise handling",
            topSpeed = 0.7f,
            acceleration = 0.65f,
            handling = 0.85f,
            braking = 0.75f,
            unlockState = CarouselItem.ItemType.Unlocked
        });
    }

    private void Start()
    {
        CreateExampleCars();
        SetupCarousel();
        
        // Subscribe to selection changed event
        if (carousel != null)
        {
            carousel.onSelectionChanged.AddListener(OnCarouselSelectionChanged);
        }
    }
    
    private void OnDestroy()
    {
        if (carousel != null)
        {
            carousel.onSelectionChanged.RemoveListener(OnCarouselSelectionChanged);
        }
    }

    private void SetupCarousel()
    {
        if (carousel == null || carItemPrefab == null || contentParent == null)
        {
            Debug.LogError("Missing required components for carousel setup!");
            return;
        }
        
        // Clear existing children in content parent
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        
        // Create car items in the carousel
        for (int i = 0; i < exampleCars.Count; i++)
        {
            var carData = exampleCars[i];
            
            // Create a new car item
            GameObject carItemObj = Instantiate(carItemPrefab, contentParent);
            CarouselItem carItem = carItemObj.GetComponent<CarouselItem>();
            
            if (carItem != null)
            {
                // Set car item properties
                carItem.ItemName = carData.carName;
                carItem.ItemDescription = carData.carDescription;
                carItem.ItemIcon = carData.carIcon;
                carItem.State = carData.unlockState;
                carItem.ItemId = carData.carId;
                
                // Add click handler
                carItem.onItemClicked.AddListener(OnCarItemClicked);
            }
        }
    }

    private void OnCarouselSelectionChanged(int index)
    {
        if (index >= 0 && index < exampleCars.Count)
        {
            var car = exampleCars[index];
            
            // Update UI elements with car data
            if (selectionText != null)
                selectionText.text = car.carName;
                
            if (statsText != null)
                statsText.text = $"Speed: {Mathf.RoundToInt(car.topSpeed * 100)}%\n" +
                                 $"Acceleration: {Mathf.RoundToInt(car.acceleration * 100)}%\n" +
                                 $"Handling: {Mathf.RoundToInt(car.handling * 100)}%\n" +
                                 $"Braking: {Mathf.RoundToInt(car.braking * 100)}%";
                
            if (previewImage != null && car.carPreviewImage != null)
                previewImage.sprite = car.carPreviewImage;
        }
    }

    private void OnCarItemClicked(CarouselItem item)
    {
        // Find the index of the clicked item
        for (int i = 0; i < carousel.transform.childCount; i++)
        {
            CarouselItem currentItem = carousel.transform.GetChild(i).GetComponent<CarouselItem>();
            if (currentItem == item)
            {
                carousel.GoToItem(i);
                break;
            }
        }
    }
    
    // UI Button callbacks
    public void OnPreviousButtonClicked()
    {
        if (carousel != null)
            carousel.Previous();
    }
    
    public void OnNextButtonClicked()
    {
        if (carousel != null)
            carousel.Next();
    }
    
    public void OnSelectButtonClicked()
    {
        int selectedIndex = carousel.GetCurrentIndex();
        if (selectedIndex >= 0 && selectedIndex < exampleCars.Count)
        {
            var car = exampleCars[selectedIndex];
            Debug.Log($"Selected car: {car.carName} (ID: {car.carId})");
            
            // Here you would typically load the race with the selected car
        }
    }
}
