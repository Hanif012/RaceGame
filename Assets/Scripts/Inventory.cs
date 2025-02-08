using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Items")]
    public Dictionary<Item, int> items = new();
    public int maxSlots = 20;

    [Header("UI")]
    public Transform itemsParent; // Parent for inventory slots
    public GameObject slotPrefab; // Prefab for individual slots

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public bool AddItem(string itemName)
    {
        Item itemToAdd = null;
        foreach (var item in items)
        {
            if (item.Key.itemName == itemName)
            {
                itemToAdd = item.Key;
                break;
            }
        }

        if (itemToAdd == null)
        {
            Debug.LogError($"Item '{itemName}' not found in the database.");
            return false; 
        }
        return AddItem(itemToAdd);
    }

    public bool AddItem(Item item, int quantity = 1)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        if (items.ContainsKey(item))
        {
            Debug.LogWarning("Inventory is full!");
            return false;
        }

        if (items.ContainsKey(item))
        {
            items[item] += quantity;
        }
        else
        {
            items.Add(item, quantity);
        }

        UpdateUI();
        return true;
    }

    public void RemoveItem(Item item, int quantity = 1)
    {
        if (!items.ContainsKey(item)) return;

        items[item] -= quantity;
        if (items[item] <= 0)
        {
            items.Remove(item);
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var kvp in items)
        {
            GameObject slot = Instantiate(slotPrefab, itemsParent);
            var slotUI = slot.GetComponent<ItemSlot>();
            slotUI.SetItem(kvp.Key, kvp.Value);
        }
    }
}
