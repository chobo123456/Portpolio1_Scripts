using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    private Canvas mainCanvas;

    private void OnEnable()
    {
        mainCanvas = GetComponent<Canvas>();

        EventBus.Sub<bool>("MainCanvasActive", MainCanvasActive);
    }

    private void OnDisable()
    {
        EventBus.UnSub<bool>("MainCanvasActive", MainCanvasActive);
    }

    private void MainCanvasActive(bool active)
    {
        mainCanvas.enabled = active;
    }
}
