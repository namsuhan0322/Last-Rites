using UnityEngine;
using UnityEngine.UI;

public class LoadingUIController : MonoBehaviour
{
    [Header("UI Components")]
    public Slider progressBar;

    private void Update()
    {
        if (ScenesManager.Instance == null) return;

        float targetProgress = ScenesManager.Instance.LoadingProgress;

        if (progressBar != null)
        {
            progressBar.value = Mathf.Lerp(progressBar.value, targetProgress, Time.deltaTime * 10f);
        }
    }
}