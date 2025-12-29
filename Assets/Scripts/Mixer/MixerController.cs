using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MixerController : MonoBehaviour
{
    #region 레퍼런스
    [Header("Mixer")]
    [SerializeField] private AudioMixer _audioMixer;

    [Header("Slider")]
    [SerializeField] private Slider _musicMasterSlider;
    [SerializeField] private Slider _musicBGMSlider;
    [SerializeField] private Slider _musicSFXSlider;

    #endregion

    #region 초기화
    private void Awake()
    {
        float masterVol = PlayerPrefs.GetFloat("Volume_Master", 1f);
        float bgmVol = PlayerPrefs.GetFloat("Volume_BGM", 1f);
        float sfxVol = PlayerPrefs.GetFloat("Volume_SFX", 1f);

        _musicMasterSlider.value = masterVol;
        _musicBGMSlider.value = bgmVol;
        _musicSFXSlider.value = sfxVol;

        _musicMasterSlider.onValueChanged.AddListener(SetMasterVolume);
        _musicBGMSlider.onValueChanged.AddListener(SetBGMVolume);
        _musicSFXSlider.onValueChanged.AddListener(SetSFXVolume);

        SetMasterVolume(masterVol);
        SetBGMVolume(bgmVol);
        SetSFXVolume(sfxVol);
    }
    #endregion

    #region 볼륨 조절
    private float GetDecibel(float volume)
    {
        return Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;
    }

    public void SetMasterVolume(float volume)
    {
        _audioMixer.SetFloat("Master", GetDecibel(volume));
        PlayerPrefs.SetFloat("Volume_Master", volume);
    }

    public void SetBGMVolume(float volume)
    {
        _audioMixer.SetFloat("BGM", GetDecibel(volume));
        PlayerPrefs.SetFloat("Volume_BGM", volume);
    }

    public void SetSFXVolume(float volume)
    {
        _audioMixer.SetFloat("SFX", GetDecibel(volume));
        PlayerPrefs.SetFloat("Volume_SFX", volume);
    }

    #endregion
}