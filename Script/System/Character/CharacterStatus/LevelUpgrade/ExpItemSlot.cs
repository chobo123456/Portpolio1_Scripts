using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpItemSlot : MonoBehaviour
{
    private int _itemId;
    private TextMeshProUGUI itemAmount;
    private Image itemIcon, itemTierColor;
    private Button addButton, removeButton;
    private bool isLock = false, isSelected = false;

    public void Initialize(int itemId, Sprite expItemIcon, ItemTier itemTier)
    {
        _itemId = itemId;

        itemAmount      = transform.Find("ItemAmount").GetComponent<TextMeshProUGUI>();
        itemIcon        = transform.Find("ItemIcon").GetComponent<Image>();
        itemTierColor   = transform.Find("ItemTierImage").GetComponent<Image>();
        addButton       = GetComponent<Button>();
        removeButton    = transform.Find("removeButton").GetComponent<Button>();

        itemIcon.sprite = expItemIcon;
        itemTierColor.color = GetTierColor(itemTier);
        addButton.onClick.AddListener(AddExp);
        removeButton.onClick.AddListener(RemoveExp);
        removeButton.gameObject.SetActive(false);
    }   

    private Color GetTierColor(ItemTier tier)
    {
        switch(tier)
        {
            case ItemTier.Legendary :
                return Color.orange;
            case ItemTier.Epic :
                return Color.purple;
            case ItemTier.Rare :
                return Color.cyan;
            case ItemTier.Common :
                return Color.gray;
            default :
                return Color.gray;
        }
    }

    private void AddExp()
    {
        if(isLock) return;

        EventBus.Invoke<int>("AddExp", _itemId);

        if(GameState.IsTutorial()) 
            EventBus.Invoke("On_Growth_UI_ClickedExpItem");
    }

    private void RemoveExp()
    {
        if(isLock) return;
        
        EventBus.Invoke<int>("RemoveExp", _itemId);
    }

    public void UpdateUI(int selectedAmount, int currentAmount)
    {
        bool slotActive = this.gameObject.activeSelf;
        bool removeButtonActive = removeButton.gameObject.activeSelf;
        
        if(currentAmount > 0 && !slotActive) 
            this.gameObject.SetActive(true);
        else if(currentAmount <= 0 && slotActive) 
            this.gameObject.SetActive(false);

        if(selectedAmount > 0) isSelected = true;
        else isSelected = false;
        
        itemAmount.SetText($"{selectedAmount}/{currentAmount}");         

        if(isSelected && !removeButtonActive) 
            removeButton.gameObject.SetActive(true);
        else if(!isSelected && removeButtonActive) 
            removeButton.gameObject.SetActive(false);
    }

    public void LockClick(bool isLock)
    {
        this.isLock = isLock;
    }

    public void DisableRemoveButton()
    {
        if(removeButton.gameObject.activeSelf) 
            removeButton.gameObject.SetActive(false);
    }
}
