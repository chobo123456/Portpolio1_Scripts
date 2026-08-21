using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI_SelectChoicePanel
{
    private ShopUI_ItemIcon icon;
    private int currentAmount = 1;

    //ui
    private Button upButton, maxUpButton, downButton, maxDownButton, acceptButton,  declineButton;
    private TextMeshProUGUI amountText;
    private GameObject mainPanel;
    
    public ShopUI_SelectChoicePanel(Transform targetTr)
    {
        Transform tr = targetTr.FindTarget("amountPanel");

        mainPanel = tr.Find("amountMainPanel").gameObject;
        mainPanel.SetActive(false);

        amountText          = mainPanel.transform.Find("Amount").GetComponentInChildren<TextMeshProUGUI>();

        upButton            = mainPanel.transform.Find("UpButton").GetComponent<Button>();
        maxUpButton         = mainPanel.transform.Find("MaxUpButton").GetComponent<Button>();
        downButton          = mainPanel.transform.Find("DownButton").GetComponent<Button>();
        maxDownButton       = mainPanel.transform.Find("MaxDownButton").GetComponent<Button>();
        acceptButton        = mainPanel.transform.Find("AcceptButton").GetComponent<Button>();
        declineButton        = mainPanel.transform.Find("DeclineButton").GetComponent<Button>();

        upButton.onClick.AddListener(Up);
        maxUpButton.onClick.AddListener(MaxUp);
        downButton.onClick.AddListener(Down);
        maxDownButton.onClick.AddListener(MaxDown);
        acceptButton.onClick.AddListener(OnClickAcceptButton);
        declineButton.onClick.AddListener(ForceInactive);
    }

    public void ShowAmountPanel(ShopUI_ItemIcon icon)
    {
        this.icon = icon;

        mainPanel.SetActive(true);

        currentAmount = 1;
        SetText();
    }
    public void ForceInactive()
    {
        mainPanel.SetActive(false);
    }
    private void OnClickAcceptButton()
    {
        mainPanel.SetActive(false);
        EventBus.Invoke<(ShopUI_ItemIcon, int)>("ShopUI_AddList", (icon, currentAmount));
    }

    private void Up()
    {
        if(icon == null) return;

        currentAmount = Mathf.Clamp(currentAmount + 1, 1, icon.itemAmount);
        SetText();
    }
    private void MaxUp()
    {
        if(icon == null) return;

        currentAmount = icon.itemAmount;
        SetText();
    }
    private void Down()
    {
        if(icon == null) return;

        currentAmount = Mathf.Clamp(currentAmount - 1, 1, icon.itemAmount);
        SetText();
    }
    private void MaxDown()
    {
        if(icon == null) return;

        currentAmount = 1;
        SetText();
    }

    private void SetText()
    {
        amountText.SetText($"{currentAmount}");
    }
}
