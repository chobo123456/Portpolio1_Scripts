using UnityEngine;
using UnityEngine.UI;

public class CharacterPartyIcon
{
    private Image _chooseImage;

    public CharacterPartyIcon(Transform parentTr, Sprite characterIcon, Sprite elementIcon)
    {
        if(_chooseImage == null) _chooseImage = parentTr.FindTarget("Icon_ChooseIcon").GetComponent<Image>();

        parentTr.FindTarget("Icon_Sprite_Front").GetComponent<Image>().sprite = characterIcon;
        parentTr.FindTarget("Icon_ElementIcon").GetComponent<Image>().sprite = elementIcon; 

        _chooseImage.enabled = false;
    }
    
    public void ShowChooseImage()
    {
        _chooseImage.enabled = true;
    }

    public void UnShowChooseImage()
    {
        _chooseImage.enabled = false;
    }
}