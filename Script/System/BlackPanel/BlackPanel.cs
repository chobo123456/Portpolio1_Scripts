using UnityEngine;
using UnityEngine.UI;

public class BlackPanel : MonoBehaviour
{
    private Image blackPanel;
    private bool fadeFinish;

    private void OnEnable()
    {
        blackPanel = transform.Find("Black").GetComponent<Image>();
        EventBus.Sub<bool, float>("BlackPanelFade", Fade);
        EventBus.Sub_Func<bool>("FadeFinish", IsFadeFinish);
    }  

    private bool IsFadeFinish() => fadeFinish;

    private void OnDisable()
    {
        EventBus.UnSub<bool, float>("BlackPanelFade", Fade);
        EventBus.UnSub_Func<bool>("FadeFinish", IsFadeFinish);
    }

    private void Fade(bool isIn, float speed = 0.25f)
    {
        fadeFinish = false;
        blackPanel.gameObject.SetActive(true);

        float start = isIn ? 1f : 0f;
        float end = isIn ? 0f : 1f;

        this.RunRoutine(Loop(start, end, speed), "BlackPanel");
    }

    System.Collections.IEnumerator Loop(float start, float end, float speed)
    {
        float percent = 0f, delta = 0f;
        while(percent < 1)
        {
            delta += Time.deltaTime;
            percent = delta / speed;

            float alpha = Mathf.Lerp(start, end, percent);
            Color c = blackPanel.color;
            c.a = alpha;

            blackPanel.color = c;

            yield return null;   
        }

        Color endColor = blackPanel.color;
        endColor.a = end;
        blackPanel.color = endColor;
        
        if(start == 1)
            blackPanel.gameObject.SetActive(false);

        fadeFinish = true;
    }
}
