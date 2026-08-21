using UnityEngine;

[System.Serializable]
public struct CurrentPlayerInfo
{
    //status
    public Vector3 playerSpeed;
    public Vector3 playerPos;
    public Quaternion playerRotate;

    //sceneId
    public int playerSceneId;
}

//임시 클래스
public static class PlayerMatch
{
    private static CurrentPlayerInfo currentPlayerInfo;

    //PlayerPos
    public static void SetPlayerPos(Vector3 playerPos)
    {
        currentPlayerInfo.playerPos = playerPos;
    }

    public static Vector3 GetPlayerPos()
    {
        return currentPlayerInfo.playerPos;
    }

    //PlayerRotate
    public static void SetPlayerRotate(Quaternion rotate)
    {
        currentPlayerInfo.playerRotate = rotate;
    }

    public static Quaternion GetPlayerRotate()
    {
        return currentPlayerInfo.playerRotate;
    }

    //SceneId

    public static void SetSceneId(int sceneId)
    {
        currentPlayerInfo.playerSceneId = sceneId;
    }
    
    public static int GetSceneId()
    {
        return currentPlayerInfo.playerSceneId;
    }
}