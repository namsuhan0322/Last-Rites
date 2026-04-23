using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BossClearManager : MonoBehaviour
{
    [Header("클리어 패널 UI")]
    public GameObject clearPanel;
    public CanvasGroup canvasGroup;       // 투명도 조절용
    public RectTransform panelTransform;  // 크기 조절용
    public GameObject bossHp;

    [Header("타이밍 설정")]
    [Tooltip("보스가 죽고 텍스트가 뜨기 전까지의 대기 시간 (초)")]
    public float startDelay = 2.0f;

    [Tooltip("텍스트가 서서히 나타나는 시간 (초)")]
    public float fadeDuration = 2.5f;

    [Tooltip("나타날 때의 시작 크기 (0.8~0.9 정도로 하면 서서히 다가오는 느낌이 납니다)")]
    public float startScale = 0.85f;
    [Tooltip("최종 크기")]
    public float endScale = 1.0f;

    [Tooltip("글자가 완전히 나타난 후 로비로 가기 전까지의 대기 시간 (초)")]
    public float stayDuration = 3.0f;

    private void Start()
    {
        if (clearPanel != null) clearPanel.SetActive(false);
    }

    public void ShowClearSequence()
    {
        StartCoroutine(ClearRoutine());
    }

    private IEnumerator ClearRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        bossHp.SetActive(false);

        clearPanel.SetActive(true);
        canvasGroup.alpha = 0f;
        panelTransform.localScale = Vector3.one * startScale;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float progress = timer / fadeDuration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            canvasGroup.alpha = smoothProgress;
            panelTransform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one * endScale, smoothProgress);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        panelTransform.localScale = Vector3.one * endScale;

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