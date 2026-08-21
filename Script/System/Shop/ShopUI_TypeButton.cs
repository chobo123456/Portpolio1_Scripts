using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI_TypeButton : MonoBehaviour
{
    private ShopType currentType;
    private Button button;
    private TextMeshProUGUI text;
    public void Initialize(ShopType type, string name = "")
    {
        currentType = type;

        if(button == null) {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        if(text == null) {
            text = GetComponentInChildren<TextMeshProUGUI>();
            text.SetText(name);
        }
    }

    private void OnClick()
    {
        EventBus.Invoke<ShopType>("ShopUI_OnClick_ShopTypeButton", currentType);
    }
}
