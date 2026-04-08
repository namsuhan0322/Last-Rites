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

        float mVol = data.masterVolume / 10f;
        float bVol = data.bgmVolume / 10f;
        float sVol = data.sfxVolume / 10f;

        // 저장된 값으로 초기화
        masterSlider.Initialize(mVol, SetMasterVolume);
        bgmSlider.Initialize(bVol, SetBGMVolume);
        sfxSlider.Initialize(sVol, SetSFXVolume);

        // 믹서에도 적용
        UpdateMixer("Master", mVol);
        UpdateMixer("BGM", bVol);
        UpdateMixer("SFX", sVol);

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
        int volumeInt = Mathf.RoundToInt(volume * 10f);

        if (SaveDataHolder.Instance.currentData.masterVolume != volumeInt)
        {
            SaveDataHolder.Instance.currentData.masterVolume = volumeInt;
            SaveDataHolder.Instance.HasChanges = true;
            UpdateMixer("Master", volumeInt / 10f);
        }
    }

    public void SetBGMVolume(float volume)
    {
        int volumeInt = Mathf.RoundToInt(volume * 10f);

        if (SaveDataHolder.Instance.currentData.bgmVolume != volumeInt)
        {
            SaveDataHolder.Instance.currentData.bgmVolume = volumeInt;
            SaveDataHolder.Instance.HasChanges = true;
            UpdateMixer("BGM", volumeInt / 10f);
        }
    }

    public void SetSFXVolume(float volume)
    {
        int volumeInt = Mathf.RoundToInt(volume * 10f);

        if (SaveDataHolder.Instance.currentData.sfxVolume != volumeInt)
        {
            SaveDataHolder.Instance.currentData.sfxVolume = volumeInt;
            SaveDataHolder.Instance.HasChanges = true;
            UpdateMixer("SFX", volumeInt / 10f);
        }
    }

    private void UpdateMixer(string param, float volume)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(param, Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
    }

    #endregion
}