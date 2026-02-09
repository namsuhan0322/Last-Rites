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

    void Start()
    {
        InitUI();
    }

    public void InitUI()
    {
        SaveData data = SaveDataHolder.Instance.currentData;

        // 저장된 값으로 초기화
        masterSlider.Initialize(data.masterVolume, SetMasterVolume);
        bgmSlider.Initialize(data.bgmVolume, SetBGMVolume);
        sfxSlider.Initialize(data.sfxVolume, SetSFXVolume);

        // 믹서에도 적용
        UpdateMixer("Master", data.masterVolume);
        UpdateMixer("BGM", data.bgmVolume);
        UpdateMixer("SFX", data.sfxVolume);

        // UI 선택 상태
        masterSlider.onSelected = OnSliderSelected;
        bgmSlider.onSelected = OnSliderSelected;
        sfxSlider.onSelected = OnSliderSelected;
        OnSliderSelected(masterSlider);

        SaveDataHolder.Instance.HasChanges = false;
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
    public void SetMasterVolume(float volume)
    {
        if (SaveDataHolder.Instance.currentData.masterVolume != volume)
        {
            SaveDataHolder.Instance.currentData.masterVolume = volume;
            SaveDataHolder.Instance.HasChanges = true;
            UpdateMixer("Master", volume);
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (SaveDataHolder.Instance.currentData.bgmVolume != volume)
        {
            SaveDataHolder.Instance.currentData.bgmVolume = volume;
            SaveDataHolder.Instance.HasChanges = true;
            UpdateMixer("BGM", volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (SaveDataHolder.Instance.currentData.sfxVolume != volume)
        {
            SaveDataHolder.Instance.currentData.sfxVolume = volume;
            SaveDataHolder.Instance.HasChanges = true;
            UpdateMixer("SFX", volume);
        }
    }

    private void UpdateMixer(string param, float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(param, Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
    }

    #endregion
}