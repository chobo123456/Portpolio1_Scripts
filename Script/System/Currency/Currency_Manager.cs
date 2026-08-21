using UnityEngine;
using System;

[System.Serializable]
public struct CurrencyStruct
{
    public int currency;
}
public class Currency_Manager : MonoBehaviour
{
    private Currency_ViewModel viewModel;

    private void OnEnable()
    {
        viewModel = new(new Currency_Model());

        EventBus.Sub<Action<int>>("TryCurrencySystem_Sub", TrySub);
        EventBus.Sub<Action<int>>("TryCurrencySystem_UnSub", TryUnSub);
        EventBus.Sub<int>("SetCurrency", SetCurrency);
        EventBus.Sub_Func<int>("GetCurrentCurrency", GetCurrency);
    }

    private void OnDisable()
    {
        EventBus.UnSub<Action<int>>("TryCurrencySystem_Sub", TrySub);
        EventBus.UnSub<Action<int>>("TryCurrencySystem_UnSub", TryUnSub);
        EventBus.UnSub<int>("SetCurrency", SetCurrency);
        EventBus.UnSub_Func<int>("GetCurrentCurrency", GetCurrency);
    }

    private void TrySub(Action<int> method)
    {
        viewModel.Currency.Subscribe(method);

        SetCurrency(0);
    }

    private void TryUnSub(Action<int> method)
    {
        viewModel.Currency.UnSubscribe(method);
    }

    private void SetCurrency(int amount)
    {
        viewModel.SetCurrency(amount);
    }

    private int GetCurrency() => viewModel.Currency.Value;
}

public class Currency_Model
{
    private Save<CurrencyStruct> save;
    public int currency {get; private set;}
    
    public Currency_Model()
    {
        save = new("Player/Currency", "CurrencyAmount");

        if(save.IsExist())
        {
            currency = save.savedData.currency;
        }
    }

    public void SetCurrency(int newCurrency)
    {
        currency += newCurrency;

        CurrencyStruct strt = new CurrencyStruct();
        strt.currency = currency;

        save.Saving(strt);
    }
}

public class Currency_ViewModel
{
    private ReactiveProperty<int> currency;
    public ReadOnlyReactiveProperty<int> Currency => currency.ToReadOnlyValue();
    private readonly Currency_Model model;

    public Currency_ViewModel(Currency_Model model)
    {
        this.model = model;

        currency = new(0);

        UpdateValue();
    }

    private void UpdateValue()
    {
        this.currency.Value = model.currency;
    }

    public void SetCurrency(int amount)
    {
        model.SetCurrency(amount);
        UpdateValue();
    }
}