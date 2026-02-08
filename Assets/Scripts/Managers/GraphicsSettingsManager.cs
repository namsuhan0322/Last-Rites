using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    private List<GraphicsOptionItem> arrowItems = new List<GraphicsOptionItem>();
    private List<SliderOptionItem> sliderItems = new List<SliderOptionItem>();

    // 옵션 데이터 목록
    private readonly List<string> resolutionOptions = new List<string> { "3840x2160", "2560x1440", "1920x1080", "1280x720" };
    private readonly List<string> frameRateOptions = new List<string> { "45", "60", "120", "144" };
    private readonly List<string> displayModeOptions = new List<string> { "Full Screen", "Borderless", "Windowed" };
    private readonly List<string> motionBlurOptions = new List<string> { "ON", "OFF" };

    void Start()
    {
        arrowItems.Add(resolutionItem);
        arrowItems.Add(frameRateItem);
        arrowItems.Add(displayModeItem);
        arrowItems.Add(motionBlurItem);

        sliderItems.Add(mouseSensItem);
        sliderItems.Add(brightnessItem);
        sliderItems.Add(contrastItem);

        InitializeSettings();
    }

    void InitializeSettings()
    {
        resolutionItem.Initialize(resolutionOptions, 2, OnResolutionChanged);
        resolutionItem.onSelected = OnArrowItemSelected;

        frameRateItem.Initialize(frameRateOptions, 1, OnFrameRateChanged);
        frameRateItem.onSelected = OnArrowItemSelected;

        displayModeItem.Initialize(displayModeOptions, 0, OnDisplayModeChanged);
        displayModeItem.onSelected = OnArrowItemSelected;

        motionBlurItem.Initialize(motionBlurOptions, 0, OnMotionBlurChanged);
        motionBlurItem.onSelected = OnArrowItemSelected;

        mouseSensItem.Initialize(0.5f, OnMouseSensChanged);
        mouseSensItem.onSelected = OnSliderItemSelected;

        brightnessItem.Initialize(1.0f, OnBrightnessChanged);
        brightnessItem.onSelected = OnSliderItemSelected;

        contrastItem.Initialize(0.5f, OnContrastChanged);
        contrastItem.onSelected = OnSliderItemSelected;

        // 처음에 첫 번째 항목 선택
        OnArrowItemSelected(resolutionItem);
    }

    // --- 선택 강조 로직 ---
    void OnArrowItemSelected(GraphicsOptionItem selectedItem)
    {
        // 화살표 애들은 선택된 놈만 켜기
        foreach (var item in arrowItems) item.SetSelectedState(item == selectedItem);
        // 슬라이더 애들은 다 끄기
        foreach (var item in sliderItems) item.SetSelectedState(false);

        UpdateDescription(selectedItem.optionName, selectedItem.optionDescription);
    }

    // 슬라이더 아이템을 선택했을 때
    void OnSliderItemSelected(SliderOptionItem selectedItem)
    {
        // 화살표 애들은 다 끄기
        foreach (var item in arrowItems) item.SetSelectedState(false);
        // 슬라이더 애들은 선택된 놈만 켜기
        foreach (var item in sliderItems) item.SetSelectedState(item == selectedItem);

        UpdateDescription(selectedItem.optionName, selectedItem.optionDescription);
    }

    void UpdateDescription(string title, string content)
    {
        if (descriptionTitle != null) descriptionTitle.text = title;
        if (descriptionContent != null) descriptionContent.text = content;
    }

    // --- 슬라이더 값 변경 콜백 ---
    void OnMouseSensChanged(float value)
    {
        Debug.Log($"마우스 감도: {value * 100}%");
        // PlayerController나 Camera 스크립트에 값 전달
    }

    void OnBrightnessChanged(float value)
    {
        Debug.Log($"밝기: {value * 100}%");
        // PostProcessing이나 RenderSettings 조절
    }

    void OnContrastChanged(float value)
    {
        Debug.Log($"대비: {value * 100}%");
        // PostProcessing 조절
    }

    void OnResolutionChanged(int index)
    {
        int width = 1920;
        int height = 1080;

        switch (index)
        {
            case 0: width = 3840; height = 2160; break;
            case 1: width = 2560; height = 1440; break; 
            case 2: width = 1920; height = 1080; break; 
            case 3: width = 1280; height = 720; break; 
        }

        Screen.SetResolution(width, height, Screen.fullScreenMode);
        Debug.Log($"해상도 변경: {width}x{height}");
    }

    void OnFrameRateChanged(int index)
    {
        int fps = 60;
        switch (index)
        {
            case 0: fps = 45; break;
            case 1: fps = 60; break;
            case 2: fps = 120; break;
            case 3: fps = 144; break;
        }

        Application.targetFrameRate = fps;
        Debug.Log($"프레임 제한 변경: {fps}");
    }

    void OnDisplayModeChanged(int index)
    {
        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;

        switch (index)
        {
            case 0: mode = FullScreenMode.ExclusiveFullScreen; break; // Full Screen
            case 1: mode = FullScreenMode.FullScreenWindow; break;    // Borderless
            case 2: mode = FullScreenMode.Windowed; break;            // Windowed
        }

        Screen.fullScreenMode = mode;
        Debug.Log($"화면 모드 변경: {mode}");
    }

    void OnMotionBlurChanged(int index)
    {
        bool isOn = (index == 0); // 0번이 ON

        Debug.Log($"모션 블러 설정: {(isOn ? "ON" : "OFF")}");
    }
}