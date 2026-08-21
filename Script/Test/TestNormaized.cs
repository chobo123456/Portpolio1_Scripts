using UnityEngine;

public class TestNormaized : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Debug.Log($"{(new Vector3(193.39f, 0f, 21.40f) - new Vector3(193.90f, 0f, 21.38f)).normalized}");
    }
}
