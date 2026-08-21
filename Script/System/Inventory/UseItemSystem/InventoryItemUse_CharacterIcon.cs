using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUse_CharacterIcon : MonoBehaviour
{
    private Image icon;
    private Button button;

    public void Initialize()
    {
        if(icon == null) icon = GetComponent<Image>();
        if(button == null) button = GetComponent<Button>();   
    }

    public void SetData(int characterId, System.Action<int> action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnClick(characterId, action));

        CharacterData etcData = DataLoader.GetData<CharacterData>(DataType.Character, characterId);
        icon.sprite = etcData.characterIcon;
    }

    private void OnClick(int characterId, System.Action<int> action)
    {
        action.Invoke(characterId);
    }
}
