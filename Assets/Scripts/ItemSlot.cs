using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI quantityText;

    private Item item;

    public void SetItem(Item newItem, int quantity)
    {
        item = newItem;
        icon.sprite = item.itemImage;
        icon.enabled = true;

        quantityText.text = quantity > 1 ? quantity.ToString() : "";
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
        quantityText.text = "";
    }
}
