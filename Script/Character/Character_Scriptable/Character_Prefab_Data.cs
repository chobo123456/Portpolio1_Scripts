using UnityEngine;

[CreateAssetMenu(fileName = "Character_Prefab_Data", menuName = "Character/Prefab/Character_Prefab_Data")]
public class Character_Prefab_Data : ScriptableObject
{
    public int id;

    [Header("InParty")]
    public GameObject inParty_prefab;

    //inScene
    [Header("InScene")]
    public GameObject chracterModel;
    public AnimatorOverrideController characterAnimator;
    public Avatar characterAnimationAvatar;
}
