using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalButtonSound : SingletonMono<GlobalButtonSound>
{
    protected override bool DontDestroy => true;

    [Header("Sound Settings")]
    public string clickSoundName = "ButtonClick"; // SoundManager에 등록된 이름

    private void Start()
    {
        // 씬 로드 이벤트 연결
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 첫 번째 씬 초기화
        AddButtonListeners();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddButtonListeners();
    }

    // 현재 씬의 모든 버튼을 찾아 리스너 추가
    public void AddButtonListeners()
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (Button btn in buttons)
        {
            // 중복 등록 방지를 위해 리스너 제거 후 추가
            btn.onClick.RemoveListener(PlayClickSound);
            btn.onClick.AddListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(clickSoundName);
        }
    }
}