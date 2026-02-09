using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
// using System.Linq; // 정렬/필터링이 필요 없어져서 삭제 가능

public class GraphicsSettingsManager : MonoBehaviour
{
    [Header("Description UI")]
    public TextMeshProUGUI descriptionTitle;
    public TextMeshProUGUI descriptionContent;

    [Header("Option Items")]
    public GraphicsOptionItem resolutionItem;
    public GraphicsOptionItem frameRateItem;
    public GraphicsOptionItem displayModeItem;
    public GraphicsOptionItem motionBlurItem;

    [Header("Slider Options")]
    public SliderOptionItem brightnessItem;
    public SliderOptionItem contrastItem;

    private List<string> resolutionOptionStrings = new List<string>();
    private List<Resolution> supportedResolutions = new List<Resolution>();

    private readonly List<string> frameRateOptions = new List<string> { "60", "120", "144", "제한 없음" };
    private readonly List<string> displayModeOptions = new List<string> { "Full Screen", "Borderless", "Windowed" };
    private readonly List<string> motionBlurOptions = new List<string> { "ON", "OFF" };

    // 해상도 타겟 데이터
    private (int w, int h)[] resTargets = { (3840, 2160), (2560, 1440), (1920, 1080), (1280, 720) };

    void Start()
    {
        InitUI();
    }

    public void InitUI()
    {
        SaveData data = SaveDataHolder.Instance.currentData;

        // 1. 해상도 스트링 만들기
        resolutionOptionStrings.Clear();
        resolutionOptionStrings.Add("3840 x 2160 (4K)");
        resolutionOptionStrings.Add("2560 x 1440 (QHD)");
        resolutionOptionStrings.Add("1920 x 1080 (FHD)");
        resolutionOptionStrings.Add("1280 x 720 (HD)");

        // 2. 각 아이템 초기화
        resolutionItem.Initialize(resolutionOptionStrings, data.resolutionIndex, OnResolutionChanged);
        frameRateItem.Initialize(frameRateOptions, data.frameRateIndex, OnFrameRateChanged);
        displayModeItem.Initialize(displayModeOptions, data.displayModeIndex, OnDisplayModeChanged);
        motionBlurItem.Initialize(motionBlurOptions, data.motionBlurIndex, OnMotionBlurChanged);

        brightnessItem.Initialize(data.brightness, OnBrightnessChanged);
        contrastItem.Initialize(data.contrast, OnContrastChanged);

        // 화살표 아이템 선택 이벤트 연결
        resolutionItem.onSelected = OnArrowItemSelected;
        frameRateItem.onSelected = OnArrowItemSelected;
        displayModeItem.onSelected = OnArrowItemSelected;
        motionBlurItem.onSelected = OnArrowItemSelected;

        // 슬라이더 아이템 선택 이벤트 연결
        brightnessItem.onSelected = OnSliderItemSelected;
        contrastItem.onSelected = OnSliderItemSelected;


        // UI 선택 상태 갱신
        OnArrowItemSelected(resolutionItem);

        ApplyAllSettings(data);

        // 초기화 과정에서 값이 세팅되면서 HasChanges가 true가 될 수 있으므로,
        // 초기화가 끝난 시점에는 강제로 false
        SaveDataHolder.Instance.HasChanges = false;
    }

    // 로드된 값으로 실제 그래픽 세팅 적용
    void ApplyAllSettings(SaveData data)
    {
        OnResolutionChanged(data.resolutionIndex);
        OnFrameRateChanged(data.frameRateIndex);
        OnDisplayModeChanged(data.displayModeIndex);
        OnMotionBlurChanged(data.motionBlurIndex);
    }

    // --- 콜백 함수들 ---
    void OnResolutionChanged(int index)
    {
        // 값이 실제로 다를 때만 true로 변경
        if (SaveDataHolder.Instance.currentData.resolutionIndex != index)
        {
            SaveDataHolder.Instance.currentData.resolutionIndex = index;
            SaveDataHolder.Instance.HasChanges = true;

            var t = resTargets[index];
            Screen.SetResolution(t.w, t.h, Screen.fullScreenMode);
        }
    }

    void OnFrameRateChanged(int index)
    {
        if (SaveDataHolder.Instance.currentData.frameRateIndex != index)
        {
            SaveDataHolder.Instance.currentData.frameRateIndex = index;
            SaveDataHolder.Instance.HasChanges = true;

            int fps = -1;
            if (index == 0) fps = 60;
            else if (index == 1) fps = 120;
            else if (index == 2) fps = 144;

            Application.targetFrameRate = fps;
        }
    }

    void OnDisplayModeChanged(int index)
    {
        SaveDataHolder.Instance.currentData.displayModeIndex = index;

        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
        if (index == 1) mode = FullScreenMode.FullScreenWindow;
        else if (index == 2) mode = FullScreenMode.Windowed;

        Screen.fullScreenMode = mode;
    }

    // --- UI 선택 로직 ---
    void OnArrowItemSelected(GraphicsOptionItem selectedItem)
    {
        if (resolutionItem != null) resolutionItem.SetSelectedState(resolutionItem == selectedItem);
        if (frameRateItem != null) frameRateItem.SetSelectedState(frameRateItem == selectedItem);
        if (displayModeItem != null) displayModeItem.SetSelectedState(displayModeItem == selectedItem);
        if (motionBlurItem != null) motionBlurItem.SetSelectedState(motionBlurItem == selectedItem);

        if (brightnessItem != null) brightnessItem.SetSelectedState(false);
        if (contrastItem != null) contrastItem.SetSelectedState(false);

        UpdateDescription(selectedItem.optionName, selectedItem.optionDescription);
    }

    void OnSliderItemSelected(SliderOptionItem selectedItem)
    {
        if (resolutionItem != null) resolutionItem.SetSelectedState(false);
        if (frameRateItem != null) frameRateItem.SetSelectedState(false);
        if (displayModeItem != null) displayModeItem.SetSelectedState(false);
        if (motionBlurItem != null) motionBlurItem.SetSelectedState(false);

        if (brightnessItem != null) brightnessItem.SetSelectedState(brightnessItem == selectedItem);
        if (contrastItem != null) contrastItem.SetSelectedState(contrastItem == selectedItem);

        UpdateDescription(selectedItem.optionName, selectedItem.optionDescription);
    }

    void UpdateDescription(string title, string content)
    {
        if (descriptionTitle != null) descriptionTitle.text = title;
        if (descriptionContent != null) descriptionContent.text = content;
    }

    // --- 슬라이더/기타 콜백 ---
    void OnMotionBlurChanged(int index)
    {
        if (SaveDataHolder.Instance.currentData.motionBlurIndex != index)
        {
            SaveDataHolder.Instance.currentData.motionBlurIndex = index;
            SaveDataHolder.Instance.HasChanges = true;
        }
    }

    void OnBrightnessChanged(float value)
    {
        if (SaveDataHolder.Instance.currentData.brightness != value)
        {
            SaveDataHolder.Instance.currentData.brightness = value;
            SaveDataHolder.Instance.HasChanges = true;
        }
    }

    void OnContrastChanged(float value)
    {
        if (SaveDataHolder.Instance.currentData.contrast != value)
        {
            SaveDataHolder.Instance.currentData.contrast = value;
            SaveDataHolder.Instance.HasChanges = true;
        }
    }
}