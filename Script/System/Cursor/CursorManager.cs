using UnityEngine;

public static class CursorManager
{
    public static void CursorActive(bool active)
    {
        if(active)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
