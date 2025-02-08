using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSlot : MonoBehaviour
{
    public TextMeshProUGUI recipeNameText;
    public Button craftButton;

    private CraftingRecipe recipe;

    public void SetRecipe(CraftingRecipe recipe, bool canCraft)
    {
        this.recipe = recipe;
        recipeNameText.text = recipe.result.itemName;
        craftButton.interactable = canCraft;
        craftButton.onClick.AddListener(() => CraftingManager.Instance.Craft(recipe));
    }
}
