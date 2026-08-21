using UnityEngine;
using System;
using TMPro;
public class CurrencyUI
{
    private TextMeshProUGUI currencyText;
    public CurrencyUI(TextMeshProUGUI text)
    {
        currencyText = text;

        EventBus.Invoke<Action<int>>("TryCurrencySystem_Sub", SetText);
    }

    private void SetText(int amount)
    {
        currencyText.SetText($"{amount}");
    }

    public void Inactive()
    {
        EventBus.Invoke<Action<int>>("TryCurrencySystem_UnSub", SetText);
    }
}
