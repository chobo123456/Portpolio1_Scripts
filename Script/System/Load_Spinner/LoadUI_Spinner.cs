using UnityEngine;

public class LoadUI_Spinner : MonoBehaviour
{
    private float rotate_Per_frame = 10f;
    private Quaternion targetRot;
    private void OnEnable()
    {
        targetRot = Quaternion.Euler(new Vector3(0f, 0f, 60f));
    }
    public void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * targetRot, Time.unscaledDeltaTime * rotate_Per_frame);
    }
}
