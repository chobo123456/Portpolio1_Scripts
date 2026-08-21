using UnityEngine;

public class CharacterPin : MonoBehaviour
{
    private Transform pinTarget;
    
    private void OnEnable()
    {
        EventBus.Sub<Transform>("SetCharacterTransform", SetPinTarget);
    }

    private void OnDisable()
    {
        EventBus.UnSub<Transform>("SetCharacterTransform", SetPinTarget);
    }
    private void SetPinTarget(Transform pinTarget)
    {
        this.pinTarget = pinTarget;
    }

    private void Update()
    {
        if(pinTarget == null) return;

        transform.position = pinTarget.position + new Vector3(0f, 1.5f, 0f);

        float angleY = pinTarget.eulerAngles.y;
        transform.rotation = Quaternion.Euler(90f, angleY, 0f);
    }
}
