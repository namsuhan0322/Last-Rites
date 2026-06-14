using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class BrightnessManager : SingletonMono<BrightnessManager>
{
    protected override bool DontDestroy => true;

    private ColorAdjustments colorAdjustments;

    [Range(-2f, 2f)]
    public float currentBrightness = 0f;
    [Range(-100f, 100f)]
    public float currentContrast = 0f;

    protected override void Awake()
    {
        base.Awake();

        Debug.Log($"BrightnessManager Awake {Time.realtimeSinceStartup}");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyColorSettings();
    }

    public void UpdateBrightness(float sliderValue)
    {
        currentBrightness = sliderValue;
        ApplyColorSettings();
    }

    public void UpdateContrast(float sliderValue)
    {
        currentContrast = sliderValue;
        ApplyColorSettings();
    }

    public void ApplyColorSettings()
    {
        var volume = GameObject.FindAnyObjectByType<Volume>();
        if (volume != null && volume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.postExposure.value = currentBrightness;
            colorAdjustments.contrast.value = currentContrast;
        }
    }
}