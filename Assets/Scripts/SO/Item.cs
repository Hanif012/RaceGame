using UnityEngine;

[System.Serializable]
public class Item
{
    [Header("Item Settings")]
    [SerializeField] public string itemName;
    [SerializeField] public Sprite itemImage;
    [SerializeField] public string itemDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam nec pur";
    [SerializeField] public int cost = 10;
}
