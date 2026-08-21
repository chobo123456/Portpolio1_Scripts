using UnityEngine;

[CreateAssetMenu(fileName = "SoundDataSO", menuName = "Sound_Data/Sound")]
public class SoundDataSO : ScriptableObject
{
    public int Id;
    public AudioClip clip;
}
