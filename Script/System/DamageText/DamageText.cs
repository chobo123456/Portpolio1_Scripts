using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    private TextMeshPro text;
    private Coroutine routine, routine2;
    private bool active = false;
    public void SetText(float damageAmount)
    {
        if (text == null) text = GetComponent<TextMeshPro>();

        transform.localScale = Vector3.one;

        text.SetText($"{damageAmount:F0}");

        routine = this.RunRoutine(Loop(), routine);

        routine2 = this.RunRoutine(RotateLoop(), routine2);
    }

    public void SetColor(Color color)
    {
        text.color = color;
    }

    IEnumerator Loop()
    {
        active = true;

        Vector2 currentSize = transform.localScale;
        
        Vector3 targetSize = Vector3.zero;
        Vector2 sizeUpedScale = transform.localScale * Random.Range(9f, 10f);
        
        float currentTime = 0f, lerpTime = 0.25f, percent = 0f;

        while (percent < 1f)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / lerpTime;

            Vector2 lerpScale = Vector2.Lerp(sizeUpedScale, targetSize, percent);
            transform.localScale = new Vector3(lerpScale.x, lerpScale.y, transform.localScale.z);
            yield return null;
        }

        yield return YieldUtil.WaitForSecondsRealtime(0.25f);

        transform.localScale = currentSize;
        gameObject.SetActive(false);
        active = false;
    }

    IEnumerator RotateLoop()
    {
        while(active)
        {
            if (Camera.main == null) break;
            Vector3 direction = (Camera.main.transform.position - transform.position).normalized;
            Quaternion rotate = Quaternion.LookRotation(direction);
            Quaternion offsetValue = Quaternion.Euler(0f, -180f, 0f);

            transform.rotation = rotate * offsetValue;

            yield return null;
        }
    }
}