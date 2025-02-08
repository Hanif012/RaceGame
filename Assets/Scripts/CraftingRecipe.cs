using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Result")]
    public Item result; // Crafted item
    public int resultQuantity = 1;

    [Header("Ingredients")]
    public List<Ingredient> ingredients; // Required ingredients

    [System.Serializable]
    public class Ingredient
    {
        public Item item; // Reference to an item in the ItemDatabase
        public int quantity; // Quantity required
    }
}
