using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

/// <summary>
/// 초기화
/// </summary>
public partial class SoundManager : MonoBehaviour
{
    public int fieldBgm = 1, _battleBgm = 2;
    private AudioSource _backGroundSource;
    private AudioMixer _mixer;
    private void OnEnable()
    {
        this.RunRoutine(Booting());   
    }

    IEnumerator Booting()
    {
        Intialize_BackGroundSource();
        Initialize_AudioMixer();
        SubscribeEvent(true);

        yield return new WaitUntil(() => _mixer != null && LoadStatus.IsReady);

        _backGroundSource.outputAudioMixerGroup = _mixer.FindMatchingGroups("Master/BGM")[0];
        PlayBGM(this.transform.position, fieldBgm);
    }

    private void Intialize_BackGroundSource()
    {
        _backGroundSource = GetComponent<AudioSource>();
    }

    private async void Initialize_AudioMixer()
    {
        _mixer = await AddressableUtil.Load_Instant<AudioMixer>("MasterMixer", this.GetCancelOnDestroy());
    }

    private void SubscribeEvent(bool isSubscribe)
    {
        if(isSubscribe)
        {
            EventBus.Sub_Func<bool>("IsExistMixer", IsExistMixerGroup);
            EventBus.Sub_Func<int, AudioMixerGroup>("GetAudioMixer", GetAudioMixerGroup);
            EventBus.Sub<AudioType, float>("SetVolume", SetVolume);
            EventBus.Sub<Vector3, AudioClip>("Play_Voice", PlayVoice);
            EventBus.Sub<Vector3, int>("Play_SFX", PlaySFX);
            EventBus.Sub<Vector3, int>("Play_BGM", PlayBGM);

            EventBus.Sub("OnBattleBGM", OnBattleBGM);
            EventBus.Sub("OnFieldBGM", OnFieldBGM);
        }   
        else
        {
            EventBus.UnSub_Func<bool>("IsExistMixer", IsExistMixerGroup);
            EventBus.UnSub_Func<int, AudioMixerGroup>("GetAudioMixer", GetAudioMixerGroup);
            EventBus.UnSub<AudioType, float>("SetVolume", SetVolume);
            EventBus.UnSub<Vector3, AudioClip>("Play_Voice", PlayVoice);
            EventBus.UnSub<Vector3, int>("Play_SFX", PlaySFX);
            EventBus.UnSub<Vector3, int>("Play_BGM", PlayBGM);

            EventBus.UnSub("OnBattleBGM", OnBattleBGM);
            EventBus.UnSub("OnFieldBGM", OnFieldBGM);
        } 
    }

    private void OnDisable()
    {
        SubscribeEvent(false);
    }

    private AudioMixer GetAudioMixer()
    {
        return _mixer;    
    }
}

//BGM 1 : Music by <a href="https://pixabay.com/ko/users/franfausto-28650439/?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=119309">Fran Fausto</a> from <a href="https://pixabay.com//?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=119309">Pixabay</a>
// FranFausto 제작 Corteza De Los Sueños

//Esha BGM : Music by <a href="https://pixabay.com/ko/users/kulakovka-47183261/?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=571173">Tony Vodnik</a> from <a href="https://pixabay.com/music//?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=571173">Pixabay</a>
//  Tony Vodnik 제작 Agressive Trap

//Battle BGM : Music by <a href="https://pixabay.com/ko/users/mykola_osennykh-53957108/?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=514322">Mykola Osennykh</a> from <a href="https://pixabay.com//?utm_source=link-attribution&utm_medium=referral&utm_campaign=music&utm_content=514322">Pixabay</a>
// Mykola_Osennykh 제작 untitled

/// <summary>
/// 시스템
/// </summary>
public partial class SoundManager : MonoBehaviour
{
    private void SetVolume(AudioType type, float value)
    {
        switch(type)
        {
            case AudioType.Master:
                _mixer.SetFloat("Master_Param", value);
                break;
            case AudioType.BGM:
                _mixer.SetFloat("BGM_Param", value);
                break;
            case AudioType.SFX:
                _mixer.SetFloat("SFX_Param", value);
                break;
            case AudioType.Voice:
                _mixer.SetFloat("Voice_Param", value);
                break;
        }   
    }

    private bool IsExistMixerGroup() => _mixer != null;
    private AudioMixerGroup GetAudioMixerGroup(int id)
    {
        switch(id)
        {
            case 0 :
                AudioMixerGroup[] sfxGroup = _mixer.FindMatchingGroups("Master/SFX");
                return sfxGroup[0];
            case 1 :
                AudioMixerGroup[] voiceGroup = _mixer.FindMatchingGroups("Master/Voice");
                return voiceGroup[0];
            default:
                this.Exception("SoundManager.cs GetAudioMixerGroup(int), id is Not Valid");
                return null;
        }
    }

    private void OnBattleBGM()
    {
        this.RunRoutine(FadeInBGM(_battleBgm));
    }

    private void OnFieldBGM()
    {
        this.RunRoutine(FadeInBGM(fieldBgm));
    }

    private IEnumerator FadeInBGM(int bgmId)
    {
        AudioClip audioClip = DataLoader.GetData<AudioClip>(DataType.BGM, bgmId);
        if(_backGroundSource.clip == audioClip) yield break;

        float percent = 0f, delta = 0f, lerpTime = 1.2f;

        float startVolume = _backGroundSource.volume, endVolume = 0f;

        while(percent < 1)
        {
            delta += Time.deltaTime;
            percent = delta / lerpTime;

            _backGroundSource.volume = Mathf.Lerp(startVolume, endVolume, percent);

            yield return null;
        }

        _backGroundSource.volume = endVolume;

        percent = 0f;
        delta = 0f;
        lerpTime = 1.2f;
        startVolume = _backGroundSource.volume;
        endVolume = 1f;
        
        _backGroundSource.clip = audioClip;
        _backGroundSource.Play();

        while(percent < 1)
        {
            delta += Time.deltaTime;
            percent = delta / lerpTime;

            _backGroundSource.volume = Mathf.Lerp(startVolume, endVolume, percent);

            yield return null;
        }

        _backGroundSource.volume = endVolume;
    }

    private void PlayBGM(Vector3 soundPos, int bgmId)
    {
        AudioClip audioClip = DataLoader.GetData<AudioClip>(DataType.BGM, bgmId);

        if(_backGroundSource.clip == audioClip) return;

        _backGroundSource.transform.position = soundPos;

        _backGroundSource.Stop();
        _backGroundSource.pitch = Random.Range(0.8f, 1f);
        _backGroundSource.clip = audioClip;

        _backGroundSource.Play();
    }

    private void PlaySFX(Vector3 soundPos, int sfxId)
    {
        var audioClip = DataLoader.GetData<AudioClip>(DataType.SFX, sfxId);
        var audioSource = EventBus.Invoke_Func<int, AudioSource>("GetAudioSource", 0);

        audioSource.transform.position = soundPos;

        audioSource?.Stop();
        audioSource.pitch = Random.Range(0.8f, 1f);
        audioSource.clip = audioClip;

        audioSource?.Play();
    }

    private void PlayVoice(Vector3 soundPos, AudioClip clip)
    {
        var audioClip = clip;
        var audioSource = EventBus.Invoke_Func<int, AudioSource>("GetAudioSource", 1);

        audioSource.transform.position = soundPos;

        audioSource?.Stop();
        audioSource.pitch = 1;
        audioSource.clip = audioClip;
        audioSource?.Play();
    }
}