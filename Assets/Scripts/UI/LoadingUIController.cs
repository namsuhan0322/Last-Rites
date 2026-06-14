using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUIController : MonoBehaviour
{
    [Header("UI Components")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    [Header("Loading Tips UI")]
    public Image backgroundImage;
    public TextMeshProUGUI descriptionText;

    private void Start()
    {
        if (progressBar != null)
        {
            progressBar.value = 0f;
        }
        if (progressText != null)
        {
            progressText.text = "0%";
        }

        if (ScenesManager.Instance != null)
        {
            string nextScene = ScenesManager.Instance.TargetSceneName;
            bool isDataFound = false;

            if (ScenesManager.Instance.loadingTips != null)
            {
                foreach (var data in ScenesManager.Instance.loadingTips)
                {
                    if (data.targetSceneName == nextScene)
                    {
                        if (backgroundImage != null) backgroundImage.sprite = data.loadingImage;
                        if (descriptionText != null) descriptionText.text = data.loadingDescription;
                        isDataFound = true;
                        break;
                    }
                }
            }

            if (!isDataFound && descriptionText != null)
            {
                descriptionText.text = "세계의 데이터를 불러오는 중입니다...";
            }
        }
    }

    private void Update()
    {
        if (ScenesManager.Instance == null) return;

        float targetProgress = ScenesManager.Instance.LoadingProgress;

        if (progressBar != null)
        {
            progressBar.value = Mathf.Lerp(progressBar.value, targetProgress, Time.deltaTime * 10f);
        }

        if (progressText != null)
        {
            int percent = Mathf.RoundToInt(progressBar.value * 100f);
            progressText.text = percent.ToString() + "%";
        }
    }
}