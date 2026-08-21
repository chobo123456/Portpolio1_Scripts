using UnityEngine;

[CreateAssetMenu(fileName = "VoiceDataSO", menuName = "Character/VoiceDataSO")]
public class VoiceDataSO : ScriptableObject
{
    public int characterId;

    public AttackVoiceClips[] attackVoiceClips;
    public AudioClip[] jumpAttackVoiceClips;
    public AudioClip[] skillVoiceClips;
}

[System.Serializable]
public struct AttackVoiceClips
{
    public AudioClip[] clips;
}