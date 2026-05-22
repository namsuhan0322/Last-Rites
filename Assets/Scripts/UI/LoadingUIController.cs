using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUIController : MonoBehaviour
{
    [Header("UI Components")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;

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