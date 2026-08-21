using UnityEngine;

public class CharacterAnimationOverrider
{
    private Animator animator;
    private MonoBehaviour mono;

    public void InjectParameter(MonoBehaviour mono, GameObject character)
    {
        this.mono       = mono;

        animator = character.transform
                   .FindTarget("Mesh")
                   .GetComponent<Animator>();
    }
    
    public void OverrideAnimation(int characterId)
    {
        mono.RunRoutine(Wait(characterId));
    }

    System.Collections.IEnumerator Wait(int characterId)
    {
        yield return null;
        yield return new WaitForEndOfFrame(); 
        
        var data = DataLoader.GetData<Character_Prefab_Data>(DataType.CharacterETC, characterId);
        
        animator.runtimeAnimatorController = data.characterAnimator;
        animator.avatar                    = data.characterAnimationAvatar;

        yield return null;

        animator.Rebind();
    }
}