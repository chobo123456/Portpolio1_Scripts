using UnityEngine;

public abstract class AnimationActBase
{
    protected PlayerDataBox box;
    public int Priority { get; protected set; }
    public AnimationActBase(PlayerDataBox _box) { box = _box; }

    public virtual bool IsFinish() { return false; }
    public virtual void OnEnterAnim() {}
    public virtual void OnUpdate() { }
    public virtual void OnExitAnim() {}
    public virtual void SetFloat(float value) {}
    public virtual void SetFloat2(float value1, float value2) {}
    public virtual float GetFuncEventTime(string clipName, string funcName)
    {
        var runtimeAnimator = box.animator.runtimeAnimatorController;

        foreach(var clip in runtimeAnimator.animationClips)
        {
            if(clip.name.Equals(clipName))
            {
                for(int i = 0; i < clip.events.Length; i++)
                {
                    AnimationEvent animEvent = clip.events[i];

                    if(animEvent.functionName == funcName)
                    {
                        float normalizedTime = animEvent.time / clip.length;

                        return normalizedTime;
                    }
                }
            }
        }
        
        return 0f;
    }
    public virtual bool CanInputBuffer() { return false; }
}