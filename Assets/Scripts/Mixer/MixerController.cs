using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class MixerController : MonoBehaviour
{
    [Header("Description UI")]
    public TextMeshProUGUI descriptionTitle;
    public TextMeshProUGUI descriptionContent;

    [Header("Slider Items")]
    public SliderOptionItem masterSlider;
    public SliderOptionItem bgmSlider;
    public SliderOptionItem sfxSlider;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    private void Start()
    {
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        float masterVol = PlayerPrefs.GetFloat("Volume_Master", 1f);
        float bgmVol = PlayerPrefs.GetFloat("Volume_BGM", 1f);
        float sfxVol = PlayerPrefs.GetFloat("Volume_SFX", 1f);

        masterSlider.Initialize(masterVol, SetMasterVolume);
        masterSlider.onSelected = OnSliderSelected;

        bgmSlider.Initialize(bgmVol, SetBGMVolume);
        bgmSlider.onSelected = OnSliderSelected;

        sfxSlider.Initialize(sfxVol, SetSFXVolume);
        sfxSlider.onSelected = OnSliderSelected;

        UpdateMixer("Master", masterVol);
        UpdateMixer("BGM", bgmVol);
        UpdateMixer("SFX", sfxVol);

        OnSliderSelected(masterSlider);
    }

    private void OnSliderSelected(SliderOptionItem selectedItem)
    {
        masterSlider.SetSelectedState(masterSlider == selectedItem);
        bgmSlider.SetSelectedState(bgmSlider == selectedItem);
        sfxSlider.SetSelectedState(sfxSlider == selectedItem);

        if (descriptionTitle != null) descriptionTitle.text = selectedItem.optionName;
        if (descriptionContent != null) descriptionContent.text = selectedItem.optionDescription;
    }

    #region 볼륨 조절 로직
    private float GetDecibel(float volume)
    {
        return Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;
    }

    private void UpdateMixer(string parameterName, float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat(parameterName, GetDecibel(volume));
        }
    }

    public void SetMasterVolume(float volume)
    {
        UpdateMixer("Master", volume);
        PlayerPrefs.SetFloat("Volume_Master", volume);
        Debug.Log($"Master Volume: {volume * 100:F0}%");
    }

    public void SetBGMVolume(float volume)
    {
        UpdateMixer("BGM", volume);
        PlayerPrefs.SetFloat("Volume_BGM", volume);
        Debug.Log($"BGM Volume: {volume * 100:F0}%");
    }

    public void SetSFXVolume(float volume)
    {
        UpdateMixer("SFX", volume);
        PlayerPrefs.SetFloat("Volume_SFX", volume);
        Debug.Log($"SFX Volume: {volume * 100:F0}%");
    }

    #endregion
}