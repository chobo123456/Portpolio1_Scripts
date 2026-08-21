using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI_ItemIcon : MonoBehaviour
{
    public ItemData itemData {get; private set;}
    public int itemAmount {get; private set;}
    private Image iconImage;
    private TextMeshProUGUI amountText;
    private Button button;
    private bool isInteract = false;

    public void Initialize()
    {
        iconImage       = transform.Find("Icon").GetComponent<Image>();
        button          = GetComponent<Button>();
        amountText      = transform.Find("Amount").GetComponent<TextMeshProUGUI>();

        button.onClick.AddListener(OnClick);
    }

    public void SetItem(ItemData data, int amount)
    {
        itemData = data;
        itemAmount = amount;
        iconImage.sprite = data.itemInfo.itemIcon;
        amountText.SetText($"{itemAmount}");
    }

    public void OnSelectedText(int amount, bool isMinus = true)
    {
        if(isMinus)
            amountText.SetText($"{itemAmount}-{amount}");
        else
            amountText.SetText($"{amount}");
    }

    private void OnClick()
    {
        isInteract = !isInteract;

        EventBus.Invoke<(ShopUI_ItemIcon, bool)>("ShopUI_OnClickIcon", (this, isInteract));
    }
}
