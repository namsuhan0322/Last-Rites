using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class SFXManager : SingletonMono<SFXManager>
{
    protected override bool DontDestroy => true;

    [Header("사운드 풀 세팅")]
    public int poolSize = 15;
    private List<AudioSource> sfxPool = new List<AudioSource>();

    [Header("사운드 클립 딕셔너리")]
    public List<SoundData> soundDataList;

    private Dictionary<string, SoundData> sfxDictionary = new Dictionary<string, SoundData>();

    protected override void Awake()
    {
        base.Awake();

        InitializePool();
        InitializeDictionary();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject($"SFX_Source_{i}");
            obj.transform.SetParent(this.transform);
            AudioSource source = obj.AddComponent<AudioSource>();

            source.spatialBlend = 1f;
            source.minDistance = 5f;
            source.maxDistance = 30f;
            source.playOnAwake = false;

            sfxPool.Add(source);
        }
    }

    private void InitializeDictionary()
    {
        foreach (var data in soundDataList)
        {
            if (!sfxDictionary.ContainsKey(data.soundID))
                sfxDictionary.Add(data.soundID, data);
        }
    }

    public void PlaySFX(string soundID, Vector3 position, float volume = 1f)
    {
        if (!sfxDictionary.TryGetValue(soundID, out SoundData data))
        {
            Debug.LogWarning($"[SFXManager] {soundID} 클립을 찾을 수 없습니다!");
            return;
        }

        AudioSource source = GetAvailableSource();
        if (source != null)
        {
            source.transform.position = position;
            source.clip = data.clip;
            source.volume = volume;
            source.outputAudioMixerGroup = data.mixerGroup;

            source.Play();
        }
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying) return source;
        }

        return null;
    }
}

[System.Serializable]
public class SoundData
{
    public string soundID;
    public AudioClip clip;
    public AudioMixerGroup mixerGroup;
}