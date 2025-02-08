using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;
    public List<CraftingRecipe> recipes; // Drag and drop recipes in the Inspector
    public Transform recipeParent; // Parent for recipe slots in UI
    public GameObject recipeSlotPrefab; // Prefab for each recipe slot in the UI

    public void UpdateAvailableRecipes()
    {
        foreach (Transform child in recipeParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var recipe in recipes)
        {
            bool canCraft = CanCraft(recipe);
            GameObject slot = Instantiate(recipeSlotPrefab, recipeParent);
            var recipeSlot = slot.GetComponent<RecipeSlot>();
            recipeSlot.SetRecipe(recipe, canCraft);
        }
    }

    public bool CanCraft(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!Inventory.Instance.items.ContainsKey(ingredient.item) ||
                Inventory.Instance.items[ingredient.item] < ingredient.quantity)
            {
                return false;
            }
        }
        return true;
    }

    public void Craft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe))
        {
            Debug.Log("Cannot craft this recipe. Not enough ingredients.");
            return;
        }

        // Remove ingredients
        foreach (var ingredient in recipe.ingredients)
        {
            Inventory.Instance.RemoveItem(ingredient.item, ingredient.quantity);
        }

        // Add the result to inventory
        Inventory.Instance.AddItem(recipe.result, recipe.resultQuantity);

        Debug.Log($"Crafted {recipe.resultQuantity} {recipe.result.itemName}.");
        UpdateAvailableRecipes();
    }
}
