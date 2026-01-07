using UnityEngine;
using System.Collections;

public class EventSystemManager : SingletonMono<EventSystemManager>
{
    public bool isEventPlaying = false;

    // 이벤트 시작
    public void StartEvent()
    {
        if (isEventPlaying) return;

        isEventPlaying = true;
        GameManager.Instance?.PauseGame();

        Debug.Log("이벤트 시작");
    }

    // 이벤트 종료
    public void EndEvent()
    {
        if (!isEventPlaying) return;

        isEventPlaying = false;
        GameManager.Instance?.ResumeGame();

        Debug.Log("이벤트 종료");
    }

    // 이벤트 연출 예제
    public void PlayCutscene(float duration)
    {
        StartCoroutine(CutsceneCoroutine(duration));
    }

    private IEnumerator CutsceneCoroutine(float duration)
    {
        StartEvent();

        Debug.Log($"{duration}초 컷신 재생");
        yield return new WaitForSecondsRealtime(duration);

        EndEvent();
    }
}