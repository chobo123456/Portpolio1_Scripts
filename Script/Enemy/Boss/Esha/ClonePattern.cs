using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ClonePattern", menuName = "Enemy/ClonePattern")]
public class ClonePattern : ScriptableObject
{   
    public WaitUntil GetWaitUntil(Animator animator, string tagName, float progress, int layer)
    {
        return new WaitUntil(() => {
                var state = animator.GetCurrentAnimatorStateInfo(layer);
                return state.IsTag(tagName) && state.normalizedTime >= progress;
            });
    }

    public IEnumerator Strategy(Animator animator, int param, WaitUntil waitUntil)
    {
        animator.enabled = true;
        animator.SetTrigger(param);

        yield return waitUntil;
    }
}