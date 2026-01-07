using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TestTimeline : MonoBehaviour
{
    #region 레퍼런스
    [Header("타임라인 설정")]
    public PlayableDirector director;

    [Header("트리거 설정")]
    public string targetTag = "Player";
    public bool playOnce = true;
    private bool hasPlayed = false;

    #endregion

    #region 초기화
    void Start()
    {
        if (director == null) director = GetComponent<PlayableDirector>();
    }

    #endregion

    #region 연출
    private void OnTriggerEnter(Collider other)
    {
        if (playOnce && hasPlayed) return;

        if (other.CompareTag(targetTag))
        {
            // EventSystemManager와 연동하여 연출 시작
            StartCoroutine(PlaySequenceRoutine());
        }
    }

    private IEnumerator PlaySequenceRoutine()
    {
        hasPlayed = true;

        if (director != null)
        {
            director.Play();

            // 이벤트 시스템 활성화
            EventSystemManager.Instance?.StartEvent();

            // director.duration은 초 단위의 전체 길이입니다.
            yield return new WaitForSecondsRealtime((float)director.duration);

            // 연출 종료 후 게임 재개
            EventSystemManager.Instance?.EndEvent();
        }
    }

    #endregion
}