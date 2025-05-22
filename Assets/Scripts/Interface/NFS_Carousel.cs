using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// A carousel implementation inspired by Need For Speed: Most Wanted (2005)
/// This creates a visually similar vehicle selection carousel with dramatic positioning
/// </summary>
public class NFS_Carousel : MonoBehaviour
{
    [System.Serializable]
    public class CarouselChangedEvent : UnityEvent<int> { }

    [Header("Carousel Items")]
    [Tooltip("Container for all carousel items")]
    [SerializeField] private Transform itemContainer;
    
    [Tooltip("Prefab for carousel items (optional)")]
    [SerializeField] private GameObject itemPrefab;
    
    [Tooltip("Array of CarouselItem components")]
    [SerializeField] private CarouselItem[] carouselItems;

    [Header("Carousel Settings")]
    [Tooltip("Duration of the transition animation")]
    [SerializeField] private float transitionDuration = 0.4f;
    
    [Tooltip("Enable auto-cycling through items")]
    [SerializeField] private bool autoCycle = false;
    
    [Tooltip("Time interval between auto-cycles")]
    [SerializeField] private float autoCycleInterval = 5f;
    
    [Tooltip("Whether the carousel should loop from end to beginning")]
    [SerializeField] private bool infiniteLoop = true;

    [Tooltip("Easing function for animations")]
    [SerializeField] private Ease easeType = Ease.OutQuart;
    
    [Tooltip("Sound effect to play on item change")]
    [SerializeField] private AudioClip transitionSound;

    [Header("Position Settings")]
    [Tooltip("Distance between items on X axis")]
    [SerializeField] private float xOffset = 500f;
    
    [Tooltip("Distance items move on Y axis when not selected")]
    [SerializeField] private float yOffset = -50f;
    
    [Tooltip("Distance items move on Z axis to create depth")]
    [SerializeField] private float zOffset = 300f;
    
    [Tooltip("Scale of the focused/selected item")]
    [SerializeField] private Vector3 focusedScale = new Vector3(1f, 1f, 1f);
    
    [Tooltip("Scale of non-focused items")]
    [SerializeField] private Vector3 unfocusedScale = new Vector3(0.7f, 0.7f, 0.7f);

    [Header("Visual Settings")]
    [Tooltip("Opacity of the focused/selected item")]
    [SerializeField] private float focusedAlpha = 1.0f;
    
    [Tooltip("Opacity of items immediately beside the focused item")]
    [SerializeField] private float adjacentAlpha = 0.7f;
    
    [Tooltip("Opacity of distant items")]
    [SerializeField] private float distantAlpha = 0.4f;

    [Header("Selection UI")]
    [Tooltip("Text showing the selected item name")]
    [SerializeField] private TextMeshProUGUI selectedItemText;
    
    [Tooltip("Text showing the selected item description")]
    [SerializeField] private TextMeshProUGUI selectedItemDescription;
    
    [Tooltip("Image showing the selected item icon/stats")]
    [SerializeField] private Image selectedItemIcon;

    [Header("Navigation Controls")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Events")]
    public CarouselChangedEvent onSelectionChanged;

    // Private variables
    private int currentIndex = 0;
    private int itemCount = 0;
    private bool isAnimating = false;
    private Coroutine autoCycleCoroutine;
    private AudioSource audioSource;

    private void Awake()
    {
        // Create audio source if needed
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && transitionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Set up button listeners
        if (previousButton != null)
            previousButton.onClick.AddListener(Previous);
        
        if (nextButton != null)
            nextButton.onClick.AddListener(Next);
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (previousButton != null)
            previousButton.onClick.RemoveListener(Previous);
            
        if (nextButton != null)
            nextButton.onClick.RemoveListener(Next);
    }

    private void Start()
    {
        InitializeCarousel();

        // Start auto-cycling if enabled
        if (autoCycle)
        {
            StartAutoCycle();
        }
    }

    private void InitializeCarousel()
    {
        // Find all CarouselItems if not already set
        if (carouselItems == null || carouselItems.Length == 0)
        {
            if (itemContainer != null)
            {
                List<CarouselItem> items = new List<CarouselItem>();
                
                for (int i = 0; i < itemContainer.childCount; i++)
                {
                    CarouselItem item = itemContainer.GetChild(i).GetComponent<CarouselItem>();
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
                
                carouselItems = items.ToArray();
                Debug.Log($"Found {carouselItems.Length} carousel items in container.");
            }
            else
            {
                Debug.LogWarning("No carousel items or container assigned!");
                return;
            }
        }

        itemCount = carouselItems.Length;
        
        if (itemCount == 0)
        {
            Debug.LogError("No carousel items found!");
            return;
        }

        // Position all items immediately
        PositionItems(true);
        
        // Update UI for initial selection
        UpdateSelectionUI();
        
        // Trigger initial selection event
        onSelectionChanged?.Invoke(currentIndex);

        // Update navigation buttons
        UpdateNavigationButtons();
    }

    /// <summary>
    /// Position all items based on their index relative to the current selection
    /// </summary>
    /// <param name="instant">Whether to position instantly or animate</param>
    private void PositionItems(bool instant = false)
    {
        for (int i = 0; i < itemCount; i++)
        {
            // Calculate the position relative to the current selection
            int relativePosition = GetRelativePosition(i);
            PositionItem(carouselItems[i], relativePosition, instant);
        }
    }

    /// <summary>
    /// Position a single carousel item based on its relative position to current selection
    /// </summary>
    private void PositionItem(CarouselItem item, int relativePosition, bool instant)
    {
        if (item == null) return;

        // Calculate target position with dramatic NFS-style positioning
        Vector3 targetPosition = CalculatePosition(relativePosition);
        Vector3 targetScale = (relativePosition == 0) ? focusedScale : unfocusedScale;
        float targetAlpha = GetAlphaForPosition(relativePosition);
        
        // Ensure the RectTransform is available
        RectTransform rectTransform = item.GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            // Get or add CanvasGroup for alpha control
            CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = item.gameObject.AddComponent<CanvasGroup>();

            // Handle initial setup vs animations
            if (instant)
            {
                rectTransform.anchoredPosition3D = targetPosition;
                rectTransform.localScale = targetScale;
                canvasGroup.alpha = targetAlpha;
                
                // Ensure proper sorting order (focused item should be on top)
                rectTransform.SetSiblingIndex(Mathf.Abs(relativePosition));
            }
            else
            {
                // Animate position
                LMotion.Create(rectTransform.anchoredPosition3D, targetPosition, transitionDuration)
                    .WithEase(easeType)
                    .BindToAnchoredPosition3D(rectTransform);
                
                // Animate scale
                LMotion.Create(rectTransform.localScale, targetScale, transitionDuration)
                    .WithEase(easeType)
                    .BindToLocalScale(rectTransform);
                
                // Animate alpha
                LMotion.Create(canvasGroup.alpha, targetAlpha, transitionDuration)
                    .WithEase(easeType)
                    .BindToAlpha(canvasGroup);
                
                // Set sibling index to control render order
                rectTransform.SetSiblingIndex(Mathf.Abs(relativePosition));
            }
        }
    }

    /// <summary>
    /// Calculate position for an item based on its relative position to current selection
    /// </summary>
    private Vector3 CalculatePosition(int relativePosition)
    {
        // NFS style positioning with dramatic perspective shifts
        float x = relativePosition * xOffset;
        float y = relativePosition == 0 ? 0 : yOffset;
        float z = Mathf.Abs(relativePosition) * zOffset;
        
        // This creates the curved/arced layout typical of NFS carousels
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Get alpha value based on item's distance from focused position
    /// </summary>
    private float GetAlphaForPosition(int relativePosition)
    {
        int absPosition = Mathf.Abs(relativePosition);
        
        if (absPosition == 0) return focusedAlpha;
        if (absPosition == 1) return adjacentAlpha;
        return distantAlpha;
    }

    /// <summary>
    /// Calculate an item's position relative to the current selection
    /// </summary>
    private int GetRelativePosition(int itemIndex)
    {
        int distance = itemIndex - currentIndex;
        
        // Handle wrapping for infinite loop
        if (infiniteLoop)
        {
            if (distance > itemCount / 2)
                distance -= itemCount;
            else if (distance < -itemCount / 2)
                distance += itemCount;
        }
        
        return distance;
    }

    /// <summary>
    /// Update the UI elements showing the current selection
    /// </summary>
    private void UpdateSelectionUI()
    {
        if (currentIndex < 0 || currentIndex >= carouselItems.Length)
            return;
            
        CarouselItem currentItem = carouselItems[currentIndex];
        
        if (currentItem != null)
        {
            // Update text
            if (selectedItemText != null)
                selectedItemText.text = currentItem.ItemName;
                
            if (selectedItemDescription != null)
                selectedItemDescription.text = currentItem.ItemDescription;
                
            // Update image
            if (selectedItemIcon != null && currentItem.ItemIcon != null)
                selectedItemIcon.sprite = currentItem.ItemIcon;
        }
    }

    /// <summary>
    /// Update navigation button states based on current position
    /// </summary>
    private void UpdateNavigationButtons()
    {
        if (!infiniteLoop)
        {
            if (previousButton != null)
                previousButton.interactable = currentIndex > 0;
                
            if (nextButton != null)
                nextButton.interactable = currentIndex < itemCount - 1;
        }
    }

    /// <summary>
    /// Move to the next item
    /// </summary>
    public void Next()
    {
        if (isAnimating || itemCount <= 1) 
            return;
            
        if (currentIndex >= itemCount - 1)
        {
            if (infiniteLoop)
                GoToItem(0);
        }
        else
        {
            GoToItem(currentIndex + 1);
        }
    }

    /// <summary>
    /// Move to the previous item
    /// </summary>
    public void Previous()
    {
        if (isAnimating || itemCount <= 1) 
            return;
            
        if (currentIndex <= 0)
        {
            if (infiniteLoop)
                GoToItem(itemCount - 1);
        }
        else
        {
            GoToItem(currentIndex - 1);
        }
    }

    /// <summary>
    /// Go directly to a specific item
    /// </summary>
    public void GoToItem(int index)
    {
        if (isAnimating || index == currentIndex || index < 0 || index >= itemCount) 
            return;

        isAnimating = true;
        
        // Play transition sound if available
        if (audioSource != null && transitionSound != null)
        {
            audioSource.clip = transitionSound;
            audioSource.Play();
        }

        // Reset auto-cycle timer if enabled
        if (autoCycle)
        {
            StopAutoCycle();
            StartAutoCycle();
        }

        // Update current index
        currentIndex = index;
        
        // Update UI elements
        UpdateSelectionUI();
        
        // Position items with animation
        PositionItems(false);
        
        // Update navigation buttons
        UpdateNavigationButtons();
        
        // Trigger selection change event
        onSelectionChanged?.Invoke(currentIndex);

        // Mark animation as complete after duration
        StartCoroutine(CompleteAnimationAfterDelay());
    }

    private IEnumerator CompleteAnimationAfterDelay()
    {
        yield return new WaitForSeconds(transitionDuration);
        isAnimating = false;
    }

    /// <summary>
    /// Start automatic cycling through items
    /// </summary>
    private void StartAutoCycle()
    {
        // Stop existing coroutine if any
        StopAutoCycle();
        
        // Start new auto-cycle coroutine
        autoCycleCoroutine = StartCoroutine(AutoCycleCoroutine());
    }

    /// <summary>
    /// Stop automatic cycling
    /// </summary>
    private void StopAutoCycle()
    {
        if (autoCycleCoroutine != null)
        {
            StopCoroutine(autoCycleCoroutine);
            autoCycleCoroutine = null;
        }
    }
    
    /// <summary>
    /// Pause the auto-cycling (called externally)
    /// </summary>
    public void PauseAutoCycle()
    {
        StopAutoCycle();
    }
    
    /// <summary>
    /// Resume auto-cycling if it was enabled
    /// </summary>
    public void ResumeAutoCycle()
    {
        if (autoCycle)
        {
            StartAutoCycle();
        }
    }

    private IEnumerator AutoCycleCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoCycleInterval);
            Next();
        }
    }

    /// <summary>
    /// Add a new item to the carousel
    /// </summary>
    public CarouselItem AddItem(string name, string description, Sprite icon, CarouselItem.ItemType state = CarouselItem.ItemType.Unlocked)
    {
        if (itemPrefab == null || itemContainer == null)
        {
            Debug.LogError("Cannot add item - missing prefab or container");
            return null;
        }

        // Instantiate new item
        GameObject newItemObj = Instantiate(itemPrefab, itemContainer);
        CarouselItem newItem = newItemObj.GetComponent<CarouselItem>();
        
        if (newItem != null)
        {
            // Set item properties
            newItem.ItemName = name;
            newItem.ItemDescription = description;
            newItem.ItemIcon = icon;
            newItem.State = state;
            
            // Add to array
            System.Array.Resize(ref carouselItems, carouselItems.Length + 1);
            carouselItems[carouselItems.Length - 1] = newItem;
            itemCount++;
            
            // Reposition items
            PositionItems(true);
            
            // Update UI if needed
            UpdateNavigationButtons();
            
            return newItem;
        }
        
        return null;
    }

    /// <summary>
    /// Remove an item from the carousel
    /// </summary>
    public void RemoveItem(int index)
    {
        if (index < 0 || index >= carouselItems.Length)
            return;
            
        // Remove the item from the scene
        if (carouselItems[index] != null)
            Destroy(carouselItems[index].gameObject);
            
        // Create new array without the item
        List<CarouselItem> items = new List<CarouselItem>(carouselItems);
        items.RemoveAt(index);
        carouselItems = items.ToArray();
        itemCount--;
        
        // Adjust current index if needed
        if (currentIndex >= itemCount)
            currentIndex = itemCount - 1;
            
        // Update positioning
        PositionItems(true);
        UpdateSelectionUI();
        UpdateNavigationButtons();
    }
    
    /// <summary>
    /// Get the currently selected item
    /// </summary>
    public CarouselItem GetSelectedItem()
    {
        if (currentIndex >= 0 && currentIndex < carouselItems.Length)
            return carouselItems[currentIndex];
        return null;
    }
    
    /// <summary>
    /// Get the current selection index
    /// </summary>
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
}
