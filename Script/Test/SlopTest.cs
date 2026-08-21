using UnityEngine;

public class SlopTest : MonoBehaviour
{
    private Rigidbody rigid;
    private void OnEnable()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Check();
    }

    private void Check()
    {
        rigid.linearVelocity = Vector3.zero;
    }
}
