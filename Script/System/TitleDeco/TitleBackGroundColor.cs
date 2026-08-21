using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleBackGroundColor : MonoBehaviour
{
    public Image backGround;
    public Color targetColor, baseColor;
    private bool pingpong = true;

    private void Start()
    {
        this.RunRoutine(ColorLoop());
    }

    private IEnumerator ColorLoop()
    {
        while(true)
        {
            if(backGround == null || this.gameObject == null) yield break;

            float delta = 0f, percent = 0f, lerpTime = 2.5f;
            Color c = backGround.color;

            Color currentTargetColor = pingpong ? targetColor : baseColor;
            Color currentStartColor = pingpong ? baseColor : targetColor;

            while(percent < 1)
            {
                if(backGround == null || this.gameObject == null) yield break;

                delta += Time.deltaTime;
                percent = delta / lerpTime;

                c = Color.Lerp(currentStartColor, currentTargetColor, percent);
                backGround.color = c;

                yield return null;
            }
            
            backGround.color = targetColor;

            pingpong = !pingpong;
            
            yield return null;
        }
    }
}
