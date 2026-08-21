using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class SoundPool : PoolManagerBase<AudioSource>
{
    public SoundPool(Transform parentTr)
    {
        MonoBehaviour mono = parentTr.parent.GetComponent<MonoBehaviour>();
        mono.RunRoutine(DelayPoolInit(parentTr));
    }
    
    IEnumerator DelayPoolInit(Transform parentTr)
    {
        yield return new WaitUntil(() => EventBus.Invoke_Func<bool>("IsExistMixer"));
        PoolInitIds initInfo = new();
        initInfo.ids = new();
        initInfo.ids.Add(0);
        initInfo.ids.Add(1);

        SetPool(
            containerTr : parentTr,
            conditionMethod : Condition,
            initializeListMethod : GetList,
            initInfo : initInfo
        );

        EventBus.Sub_Func<int, AudioSource>("GetAudioSource", GetFromPool);
    }

    public void OnDisable()
    {
        EventBus.UnSub_Func<int, AudioSource>("GetAudioSource", GetFromPool);
    }

    private List<AudioSource> GetList(int id = 0)
    {
        List<AudioSource> newList = new();

        int newPoolCount = 5;
        for(int i = 0; i < newPoolCount; i++)
        {
            var obj = new GameObject($"SFX");
            obj.transform.SetParent(containerTr);
            var audioSource = obj.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.outputAudioMixerGroup = EventBus.Invoke_Func<int, AudioMixerGroup>("GetAudioMixer", id);

            newList.Add(audioSource);
        }

        return newList;
    }

    private bool Condition(AudioSource source)
    {
        return !source.isPlaying;
    }

    private AudioSource GetFromPool(int id)
    {
        return base.GetFromPool(id);
    }
}