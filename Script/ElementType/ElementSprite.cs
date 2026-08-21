using UnityEngine;

[CreateAssetMenu(fileName = "ElementSprite", menuName = "Element/ElementSprite")]
public class ElementSprite : ScriptableObject
{
    public int elementId;
    public Sprite elementSprite;
}
