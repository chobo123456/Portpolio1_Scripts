using UnityEngine;

[CreateAssetMenu(fileName = "CraftRecipe", menuName = "Craft/RecipeDataBase")]
public class CraftRecipeDataBase : ScriptableObject
{
    public CraftRecipe[] recipes;
}
