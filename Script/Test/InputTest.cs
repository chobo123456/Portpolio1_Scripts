using UnityEngine;
using UnityEngine.InputSystem;
public class InputTest : MonoBehaviour
{
    private CharacterInput _input;

    private void OnEnable()
    {
        _input = new();
        
        if(_input != null)
        {
            InputActionAsset asset = _input.asset;

            foreach(var actionMap in asset.actionMaps)
            {
                foreach(var action in actionMap.actions)
                {
                    var bindings = action.bindings;

                    for(int i = 0; i < action.bindings.Count; i++)
                    {
                        var binding = action.bindings[i];
                        
                        if(binding.isPartOfComposite)
                            Util.Log($" ㄴ 복합체 부분 : {action.bindings[i].name}");
                        else if(!binding.isComposite)
                            Util.Log($"그냥 일반 : {action.name}");
                    }
                }
            }
        }
    }
}
