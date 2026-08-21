using UnityEngine;
using UnityEngine.UI;
using System;

public class ButtonUtil<T>
{
    private Button button;
    private Action<T> method;
    private T value;
    public ButtonUtil(Transform targetTr, Action<T> method, T value)
    {
        button = targetTr.GetComponent<Button>();
        this.method = method;
        this.value  = value;

        button.onClick.AddListener(Invoke);
    }

    public void Dispose()
    {
        if(button != null)
            button.onClick.RemoveAllListeners();
    }

    private void Invoke()
    {
        method.Invoke((T)value);
    }
}

public class ButtonUtil
{
    private Button button;
    private Action method;

    public ButtonUtil(Transform targetTr, Action method)
    {
        button = targetTr.GetComponent<Button>();
        this.method = method;

        button.onClick.AddListener(Invoke);
    }

    public void Dispose()
    {
        if(button != null)
            button.onClick.RemoveAllListeners();
    }

    private void Invoke()
    {
        method.Invoke();
    }
}
