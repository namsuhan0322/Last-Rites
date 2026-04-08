using UnityEngine;

[System.Serializable]
public class SaveData
{
    // --- 그래픽 설정 ---
    [Tooltip("해상도 (0: 4K, 1: QHD, 2: FHD, 3: HD)")]
    public int resolutionIndex;
    [Tooltip("프레임 (0: 60, 1: 120...)")]
    public int frameRateIndex;
    [Tooltip("화면 모드 (0: 전체, 1: 테두리없음...)")]
    public int displayModeIndex;
    [Tooltip("모션블러 (0: ON, 1: OFF)")]
    public int motionBlurIndex;

    // --- 슬라이더 설정 ---
    public float brightness;
    public float contrast;

    // --- 사운드 설정 ---
    public int masterVolume;
    public int bgmVolume;
    public int sfxVolume;

    // 생성자 (초기화 시 기본값 설정)
    public SaveData()
    {
        resolutionIndex = -1;   // 각 PC의 모니터 해상도에 따라 디폴트 값이 달라짐
        frameRateIndex = 0;     // 60FPS 기본
        displayModeIndex = 0;   // 전체화면 기본
        motionBlurIndex = 0;    // ON 기본

        brightness = 1.0f;
        contrast = 0.5f;

        masterVolume = 10;
        bgmVolume = 10;
        sfxVolume = 10;
    }
}