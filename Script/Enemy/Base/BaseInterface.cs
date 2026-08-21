using UnityEngine;

public interface ISpawnable
{
    bool IsSpawned();
    void SetSpawn();
}

public interface ICullable
{
    Transform GetTRS();
    void ToggleOn();
    void DisableUpdate();
    void ToggleOff();
}

public interface IClone
{
    void SetCommand(CloneCommand strategy);

    bool IsActive();
    void Execute();

    void SetActive(bool isActive);

    //TRS
    void SetPosition(Vector3 position);
    void SetRotation(Quaternion rotation);

    void Exception();
}

public enum CloneCommand
{
    Dash,
    InPlace,
}