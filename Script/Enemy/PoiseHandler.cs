using UnityEngine;
using System.Collections;

public enum HitType
{
    Big,
    Small,    
}

public class PoiseHandler
{
    private readonly float maxPoise;
    private float currentPoise;

    public PoiseHandler(float maxPoise = 20f)
    {
        this.maxPoise = maxPoise;
        currentPoise = maxPoise;
    }   

    public HitType TakeImpact(float impact)
    {
        currentPoise -= impact;

        if(currentPoise <= 0) return HitType.Big;
        else return HitType.Small;
    }

    public void Reset()
    {
        currentPoise = maxPoise;
    }
}
