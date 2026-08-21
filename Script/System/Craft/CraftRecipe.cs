using UnityEngine;

[CreateAssetMenu(fileName = "CraftRecipe", menuName = "Craft/Recipe")]
public class CraftRecipe : ScriptableObject
{
    public int recipeId;

    public CraftRecipeMaterial[] recipe_material;
    
    public int result_item_Id;
}

[System.Serializable]
public struct CraftRecipeMaterial
{
    public int recipe_material_Id;
    public int needAmount;
}