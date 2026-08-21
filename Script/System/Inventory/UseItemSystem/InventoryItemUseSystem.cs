using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class InventoryItemUseSystem
{
    private readonly Transform parentTr;
    private readonly GameObject panel;
    private readonly Button closeButton;
    private readonly System.Action<int> clickEvent;
    private readonly System.Func<bool> _isCloseable;
    private List<InventoryItemUse_CharacterIcon> icons = new();
    public InventoryItemUseSystem(GameObject panelObject, System.Action<int> getItemAction, System.Func<bool> isCloseable)
    {
        panel = panelObject;
        panel.SetActive(false);

        this.parentTr = panelObject.transform.Find("Characters");
        closeButton   = panelObject.transform.Find("CloseButton").GetComponent<Button>();
        closeButton.onClick.AddListener(ClosePanel);

        clickEvent = getItemAction;
        _isCloseable = isCloseable;
    }   

    public void ActivePanel()
    {
        SetData();
        panel.SetActive(true);
    }
    
    public void Initialize()
    {
        for(int i = 0; i < parentTr.childCount; i++)
        {
            Transform childTr = parentTr.GetChild(i);

            InventoryItemUse_CharacterIcon comp = childTr.GetComponent<InventoryItemUse_CharacterIcon>();

            if(comp != null)
            {
                comp.Initialize();
                icons.Add(comp);
            }
        }
    }
    
    private void SetData()
    {
        if(icons.Count <= 0) return;

        Dictionary<int, int> currentPartyCharacterMap = 
            EventBus.Invoke_Func<Dictionary<int, int>>("CharacterPartySaveInfo_GetPartyInfo");

        if(currentPartyCharacterMap == null) return;

        List<int> characterIds = new();

        foreach(var map in currentPartyCharacterMap)
        {
            int characterId = map.Value;
            characterIds.Add(characterId);
        }

        for(int i = 0; i < icons.Count; i++)
        {
            InventoryItemUse_CharacterIcon icon = icons[i];

            if(i < characterIds.Count)
            {
                int characterId = characterIds[i];
                icon.SetData(characterId, clickEvent);
                icon.gameObject.SetActive(true);
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }
    }
    public void ClosePanel()
    {
        if(_isCloseable.Invoke()) return;

        panel.SetActive(false);
    }
}
