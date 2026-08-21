using UnityEngine;
using System.Collections;

public class TitleTextMoveSmall : MonoBehaviour
{
    public RectTransform rect;
    private Vector2 rightMaxMove, leftMaxMove, startPoint;
    public bool pingpong = false;

    private void Start()
    {
        startPoint      = rect.anchoredPosition;
        rightMaxMove    = startPoint + new Vector2(5f, 0f);
        leftMaxMove     = startPoint + new Vector2(-5f, 0f);

        this.RunRoutine(Move());
    }

    private IEnumerator Move()
    {
        while(true)
        {
            if(rect == null || this.gameObject == null) yield break;

            float delta = 0f, percent = 0f, lerpTime = UnityEngine.Random.Range(0.15f, 0.2f);

            Vector2 currentTargetPoint = pingpong ? rightMaxMove : leftMaxMove;
            
            while(percent < 1f)
            {
                if(rect == null || this.gameObject == null) yield break;

                delta += Time.deltaTime;
                percent = delta / lerpTime;

                rect.anchoredPosition = Vector2.Lerp(startPoint, currentTargetPoint, percent);
                yield return null;
            }

            delta = 0f;
            percent = 0f;

            Vector2 currentPoint = rect.anchoredPosition;

            while(percent < 1f)
            {
                if(rect == null || this.gameObject == null) yield break;

                delta += Time.deltaTime;
                percent = delta / lerpTime;

                rect.anchoredPosition = Vector2.Lerp(currentPoint, startPoint, percent);
                yield return null;
            }

            pingpong = !pingpong;

            yield return null;
        }
    }
}
