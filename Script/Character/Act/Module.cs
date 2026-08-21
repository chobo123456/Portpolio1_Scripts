using UnityEngine;

public abstract class Module : MonoBehaviour
{
    public ActBase _act;
    public abstract void SetModule(PlayerDataBox box);
}