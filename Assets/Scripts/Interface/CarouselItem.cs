using UnityEngine;

public class CarouselItem : MonoBehaviour
{
    [Header("Item Properties")]
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
    public enum ItemType {Locked, Unlocked, NULL};
}