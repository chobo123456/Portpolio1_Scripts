using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CharacterHUD : MonoBehaviour
{
    public bool IsActive {get; private set;} = false;
    private Image _backGround, _characterIcon, _elementIcon;
    private GameObject _inputTextBackground;
    private TextMeshProUGUI _inputText;
    private Color _originColor;

    public void Initialize()
    {
        if(_backGround == null) 
        {
            _backGround = GetComponent<Image>();
            _originColor = _backGround.color;
        }

        if(_inputTextBackground  == null) _inputTextBackground = transform.FindTarget("InputBack").gameObject;
        if(_characterIcon        == null) _characterIcon = transform.FindTarget("CharacterIcon").GetComponent<Image>();   
        if(_elementIcon          == null) _elementIcon = transform.FindTarget("ElementIcon").GetComponent<Image>();   
        if(_inputText            == null) _inputText = transform.FindTarget("ChangeInput").GetComponent<TextMeshProUGUI>();
    }

    public void OnSelect(bool isSelect)
    {
        if(isSelect)
        {
            Color c = Color.white;
            _backGround.color = c;
        }
        else
        {
            _backGround.color = _originColor;
        }
    }

    public void EnableSlot(Sprite characterIcon, Sprite elementIcon)
    {
        IsActive = true;
        
        _characterIcon.sprite = characterIcon;
        _elementIcon.sprite = elementIcon;
        SetUIActive(true);       
    }

    public void DisableSlot()
    {
        IsActive = false;
        SetUIActive(false);
    }

    private void SetUIActive(bool active)
    {
        _inputTextBackground.SetActive(active);
        _elementIcon.enabled = active;
        _backGround.enabled = active;
        _characterIcon.enabled = active;
        _inputText.enabled = active;
    }
}
