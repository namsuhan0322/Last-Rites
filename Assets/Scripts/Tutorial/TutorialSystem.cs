using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialSystem : MonoBehaviour
{
    [Header("시작 판넬")]
    public GameObject startPanel;
    public TMP_Text startText;
    public TMP_Text closeText;

    [Header("미션UI")]
    public TMP_Text missionText;

    public float typingSpeed = 0.05f;
    public float blinkSpeed = 2f;

    string startMessage = "우클릭 시 이동";
    string goalMessage = "목표 지점으로 이동하세요";

    bool waitingForEnter = false;
    public GameObject directionArrow;
    void Start()
    {
        Time.timeScale = 0f;

        startPanel.SetActive(true);
        missionText.gameObject.SetActive(false);
        closeText.gameObject.SetActive(false);

        StartCoroutine(TypeStartText());
    }

    void Update()
    {
        if (!waitingForEnter) return;

        Color color = closeText.color;
        color.a = Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1));
        closeText.color = color;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            CloseStartTutorial();
        }
    }

    //텍스트시작
    IEnumerator TypeStartText()
    {
        startText.text = "";

        foreach (char c in startMessage)
        {
            startText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        waitingForEnter = true;
        closeText.gameObject.SetActive(true);
    }

    //시작 튜토리얼 닫기
    void CloseStartTutorial()
    {
        waitingForEnter = false;

        startPanel.SetActive(false);
        Time.timeScale = 1f;

        missionText.gameObject.SetActive(true);

        directionArrow.SetActive(true);

        StartCoroutine(TypeMissionText());
    }


    //미션 텍스트 
    IEnumerator TypeMissionText()
    {
        missionText.text = "";

        foreach (char c in goalMessage)
        {
            missionText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }


    //골 도착
    public void ReachGoal()
    {
        directionArrow.SetActive(false);

        StartCoroutine(FadeOutMission());
    }

    //미션 사라지게 하기
    IEnumerator FadeOutMission()
    {
        float time = 0f;
        Color color = missionText.color;

        while (time < 1f)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, time);
            missionText.color = color;
            yield return null;
        }

        missionText.gameObject.SetActive(false);
    }
}
