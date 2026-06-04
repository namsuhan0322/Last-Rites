using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BossRushClearManager : MonoBehaviour
{
    [Header("클리어 패널 UI")]
    public GameObject clearPanel;
    public CanvasGroup canvasGroup;
    public RectTransform panelTransform;
    public GameObject bossHp; // 보스러쉬 공통 보스 체력바 끄기용

    [Header("타이밍 설정")]
    [Tooltip("모든 보스를 처치하고 텍스트가 뜨기 전 대기 시간")]
    public float startDelay = 2.0f;
    public float fadeDuration = 2.5f;
    public float startScale = 0.85f;
    public float endScale = 1.0f;
    public float stayDuration = 3.0f;

    private void Start()
    {
        if (clearPanel != null) clearPanel.SetActive(false);
    }

    public void ShowBossRushClearSequence()
    {
        StartCoroutine(ClearRoutine());
    }

    private IEnumerator ClearRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        if (bossHp != null) bossHp.SetActive(false);

        clearPanel.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (panelTransform != null) panelTransform.localScale = Vector3.one * startScale;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            if (canvasGroup != null) canvasGroup.alpha = smoothProgress;
            if (panelTransform != null) panelTransform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one * endScale, smoothProgress);

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (panelTransform != null) panelTransform.localScale = Vector3.one * endScale;

        yield return new WaitForSeconds(stayDuration);

        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveAllData();
        }

        if (ScenesManager.Instance != null)
        {
            ScenesManager.Instance.LoadLobbyScene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }
    }
}