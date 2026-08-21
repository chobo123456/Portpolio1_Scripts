using UnityEngine;


[CreateAssetMenu(fileName = "UseItemData", menuName = "Item/Data/Usable/HealthItem")]
public class HealthItem : UseItemData
{
    public float healAmount;

    public override void UseItem(int characterId)
    {
        var characterData = DataLoader.GetData<CharacterData>(DataType.Character, characterId);

        int currentCharacterLevel = EventBus.Invoke_Func<int, int>("GetCharacterLevel", characterId);
        float maxHp = characterData.levelStep.GetMaxHpUseLevel(currentCharacterLevel);

        float recentHp = EventBus.Invoke_Func<int, float>("GetCharacterRecentHp", characterId);
        float heal = Mathf.Clamp(recentHp + healAmount, 1f, maxHp);
        EventBus.Invoke<int, float>("OnCharacterHpChanged", characterId, heal);

        EventBus.Invoke("CharacterReloadHp");
    }
}