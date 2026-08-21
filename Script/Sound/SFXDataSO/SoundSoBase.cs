using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundSoBase", menuName = "Sound_Data/SoundSoBase")]
public class SoundSoBase : ScriptableObject
{
    public List<SoundDataSO> sounds;
}
