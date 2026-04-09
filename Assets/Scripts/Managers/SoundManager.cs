using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#region 사운드 클래스
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1.0f;

    [Range(0.1f, 3f)]
    public float pitch = 1.0f;
    public bool loop;
    public AudioMixerGroup mixerGroup;

    [HideInInspector]
    public AudioSource source;
}

#endregion

public class SoundManager : SingletonMono<SoundManager>
{
    #region 레퍼런스
    protected override bool DontDestroy => true;

    [Header("References")]
    public List<Sound> sounds = new List<Sound>();
    public AudioMixer audioMixer;

    #endregion

    #region 초기화
    protected override void Awake()
    {
        base.Awake();

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.outputAudioMixerGroup = sound.mixerGroup;
        }
    }

    #endregion

    #region 사운드

    // 일반 효과음 재생용
    public void PlaySound(string name)
    {
        Sound soundToPlay = sounds.Find(sound => sound.name == name);

        if (soundToPlay != null)
        {
            soundToPlay.source.Play();
        }
        else
        {
            Debug.LogWarning("사운드 : " + name + " 없습니다.");
        }
    }

    // 특정 사운드 정지용
    public void StopSound(string name)
    {
        Sound soundToStop = sounds.Find(sound => sound.name == name);

        if (soundToStop != null)
        {
            soundToStop.source.Stop();
        }
    }

    // BGM 전용 재생기 (기존 BGM을 다 끄고 새 BGM을 틉니다)
    public void PlayBGM(string name)
    {
        foreach (Sound s in sounds)
        {
            if (s.loop && s.source.isPlaying)
            {
                s.source.Stop();
            }
        }

        PlaySound(name);
    }

    #endregion
}