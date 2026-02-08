using UnityEngine;
using TMPro;
using System.Collections.Generic;
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
    public SliderOptionItem mouseSensItem;
    public SliderOptionItem brightnessItem;
    public SliderOptionItem contrastItem;

    private List<string> resolutionOptionStrings = new List<string>();
    private List<Resolution> supportedResolutions = new List<Resolution>();

    private readonly List<string> frameRateOptions = new List<string> { "60", "120", "144", "제한 없음" };
    private readonly List<string> displayModeOptions = new List<string> { "Full Screen", "Borderless", "Windowed" };
    private readonly List<string> motionBlurOptions = new List<string> { "ON", "OFF" };

    void Start()
    {
        InitializeSettings();
    }

    void InitializeSettings()
    {
        InitResolutions();

        // 현재 내 모니터 해상도와 일치하는 것이 있는지 찾기
        // (없으면 FHD인 2번 인덱스를 기본값으로 설정)
        int currentResIndex = 2;

        for (int i = 0; i < supportedResolutions.Count; i++)
        {
            // 너비와 높이가 현재 화면과 일치하면 그 인덱스를 선택
            if (supportedResolutions[i].width == Screen.width &&
                supportedResolutions[i].height == Screen.height)
            {
                currentResIndex = i;
                break;
            }
        }

        // 해상도 아이템 초기화
        resolutionItem.Initialize(resolutionOptionStrings, currentResIndex, OnResolutionChanged);
        resolutionItem.onSelected = OnArrowItemSelected;

        // --- 나머지 설정 초기화 ---
        frameRateItem.Initialize(frameRateOptions, 0, OnFrameRateChanged);
        frameRateItem.onSelected = OnArrowItemSelected;

        displayModeItem.Initialize(displayModeOptions, 0, OnDisplayModeChanged);
        displayModeItem.onSelected = OnArrowItemSelected;

        motionBlurItem.Initialize(motionBlurOptions, 0, OnMotionBlurChanged);
        motionBlurItem.onSelected = OnArrowItemSelected;

        // 슬라이더 초기화
        mouseSensItem.Initialize(0.5f, OnMouseSensChanged);
        mouseSensItem.onSelected = OnSliderItemSelected;

        brightnessItem.Initialize(1.0f, OnBrightnessChanged);
        brightnessItem.onSelected = OnSliderItemSelected;

        contrastItem.Initialize(0.5f, OnContrastChanged);
        contrastItem.onSelected = OnSliderItemSelected;

        // 처음에 해상도 메뉴 선택
        OnArrowItemSelected(resolutionItem);
    }

    void InitResolutions()
    {
        resolutionOptionStrings.Clear();
        supportedResolutions.Clear();

        var targets = new (int w, int h, string name)[] {
            (3840, 2160, "4K"),
            (2560, 1440, "QHD"),
            (1920, 1080, "FHD"),
            (1280, 720, "HD")
        };

        foreach (var t in targets)
        {
            Resolution res = new Resolution();
            res.width = t.w;
            res.height = t.h;
            supportedResolutions.Add(res);

            resolutionOptionStrings.Add($"{t.w} x {t.h} ({t.name})");
        }
    }

    // --- 콜백 함수들 ---
    void OnResolutionChanged(int index)
    {
        Resolution targetRes = supportedResolutions[index];
        Screen.SetResolution(targetRes.width, targetRes.height, Screen.fullScreenMode);
        Debug.Log($"해상도 변경: {targetRes.width} x {targetRes.height}");
    }

    void OnFrameRateChanged(int index)
    {
        int fps = -1;
        switch (index)
        {
            case 0: fps = 60; break;
            case 1: fps = 120; break;
            case 2: fps = 144; break;
            case 3: fps = -1; break;
        }
        Application.targetFrameRate = fps;
        Debug.Log($"프레임 변경: {fps}");
    }

    void OnDisplayModeChanged(int index)
    {
        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
        switch (index)
        {
            case 0: mode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: mode = FullScreenMode.FullScreenWindow; break;
            case 2: mode = FullScreenMode.Windowed; break;
        }
        Screen.fullScreenMode = mode;
        Debug.Log($"화면 모드: {mode}");
    }

    // --- UI 선택 로직 (기존 유지) ---
    void OnArrowItemSelected(GraphicsOptionItem selectedItem)
    {
        if (resolutionItem != null) resolutionItem.SetSelectedState(resolutionItem == selectedItem);
        if (frameRateItem != null) frameRateItem.SetSelectedState(frameRateItem == selectedItem);
        if (displayModeItem != null) displayModeItem.SetSelectedState(displayModeItem == selectedItem);
        if (motionBlurItem != null) motionBlurItem.SetSelectedState(motionBlurItem == selectedItem);

        if (mouseSensItem != null) mouseSensItem.SetSelectedState(false);
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

        if (mouseSensItem != null) mouseSensItem.SetSelectedState(mouseSensItem == selectedItem);
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
    void OnMotionBlurChanged(int index) { Debug.Log($"모션블러: {index}"); }
    void OnMouseSensChanged(float value) { Debug.Log($"감도: {value}"); }
    void OnBrightnessChanged(float value) { Debug.Log($"밝기: {value}"); }
    void OnContrastChanged(float value) { Debug.Log($"대비: {value}"); }
}