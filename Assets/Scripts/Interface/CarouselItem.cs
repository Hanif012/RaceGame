using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using LitMotion;
using LitMotion.Extensions;

public class CarouselItem : MonoBehaviour
{
    [System.Serializable]
    public class ItemClickedEvent : UnityEvent<CarouselItem> { }

    [Header("Item Properties")]
    [SerializeField] private string itemName;
    [SerializeField] private string itemDescription;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private ItemType itemState = ItemType.NULL;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Button itemButton;

    [Header("Visual Effects")]
    [SerializeField] private GameObject selectedHighlight;
    [SerializeField] private GameObject hoverEffect;
    
    [Header("Events")]
    public ItemClickedEvent onItemClicked;

    // Additional properties and custom data that can be attached to the item
    [SerializeField] private string itemId;
    [SerializeField] private int sortOrder;
    [SerializeField] private Color itemColor = Color.white;

    public enum ItemType { Locked, Unlocked, NULL };

    // Public getters and setters
    public string ItemName
    {
        get => itemName;
        set
        {
            itemName = value;
            if (nameText != null) nameText.text = value;
        }
    }

    public string ItemDescription
    {
        get => itemDescription;
        set
        {
            itemDescription = value;
            if (descriptionText != null) descriptionText.text = value;
        }
    }

    public Sprite ItemIcon
    {
        get => itemIcon;
        set
        {
            itemIcon = value;
            if (iconImage != null) iconImage.sprite = value;
        }
    }

    public ItemType State
    {
        get => itemState;
        set
        {
            itemState = value;
            UpdateVisualState();
        }
    }

    public string ItemId
    {
        get => itemId;
        set => itemId = value;
    }
    
    public int SortOrder
    {
        get => sortOrder;
        set => sortOrder = value;
    }
    
    public Color ItemColor
    {
        get => itemColor;
        set
        {
            itemColor = value;
            ApplyItemColor();
        }
    }
    
    public bool IsSelected { get; private set; }

    private void Awake()
    {
        // Set up button click handler
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnDestroy()
    {
        // Clean up button listener
        if (itemButton != null)
        {
            itemButton.onClick.RemoveListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        // Invoke the item clicked event
        onItemClicked?.Invoke(this);
    }

    private void OnEnable()
    {
        UpdateVisualState();
    }

    private void Start()
    {
        UpdateVisualState();
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        
        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(selected);
        }
    }
    
    public void SetHovered(bool hovered)
    {
        if (hoverEffect != null)
        {
            hoverEffect.SetActive(hovered);
        }
    }

    private void UpdateVisualState()
    {
        // Update UI elements based on the data
        if (nameText != null) nameText.text = itemName;
        if (descriptionText != null) descriptionText.text = itemDescription;
        if (iconImage != null && itemIcon != null) iconImage.sprite = itemIcon;
        
        // Handle locked state
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(itemState == ItemType.Locked);
        }
        
        // Apply color
        ApplyItemColor();
        
        // Disable button if locked
        if (itemButton != null)
        {
            itemButton.interactable = itemState != ItemType.Locked;
        }
    }
    
    private void ApplyItemColor()
    {
        if (iconImage != null)
        {
            iconImage.color = itemColor;
        }
    }

    // NFS-specific: Shake/pulse animation effect when selected
    public void PlaySelectAnimation()
    {
        // Simple animation to scale up briefly then back to normal
        LMotion.Create(transform.localScale, transform.localScale * 1.1f, 0.1f)
            .WithEase(Ease.OutQuad)
            .WithOnComplete(() => {
                LMotion.Create(transform.localScale, Vector3.one, 0.1f)
                    .WithEase(Ease.InQuad)
                    .BindToLocalScale(transform);
            })
            .BindToLocalScale(transform);
    }
}