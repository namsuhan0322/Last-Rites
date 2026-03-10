using System.Collections;
using UnityEngine;
using TMPro;
using Cinemachine;

public class TutorialSystem : MonoBehaviour
{
    [Header("시작 판넬")]
    public GameObject startPanel;
    public TMP_Text startText;
    public GameObject rightClickUI;   // 마우스 우클릭 아이콘
    public CanvasGroup rightClickGroup;
    public GameObject leftClickUI;
    public CanvasGroup leftClickGroup;

    [Header("미션UI")]
    public TMP_Text missionText;

    [Header("전투 튜토리얼")]
    public GameObject battlePanel;
    public TMP_Text battleText;
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera tutorialCam;

    public Transform[] spawnPoints;    // 늑대 3마리 위치


    public PlayerController playerController; // 추가
    public GameObject wolfPrefab;

    public Camera mainCamera;

    bool waitingForBattleStart = false;


    public float typingSpeed = 0.05f;
    public float blinkSpeed = 2f;

    public bool waitingForRightClick = false;

    string startMessage = "마우스 우클릭으로 이동하십시오";
    string goalMessage = "목표 지점으로 이동하세요";

    bool waitingForEnter = false;
    public GameObject directionArrow;


    bool battleTutorialShown = false;
    public bool tutorialPlaying = false;

    void Start()
    {
        tutorialPlaying = true; 

        startPanel.SetActive(true);
        missionText.gameObject.SetActive(false);
        rightClickUI.SetActive(false);
        leftClickUI.SetActive(false);
        StartCoroutine(TypeStartText());
    }

    void Update()
    {
        if (waitingForEnter)
        {
            rightClickGroup.alpha =
                Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1));

            if (Input.GetMouseButtonDown(1))
                CloseStartTutorial();
        }

        if (waitingForBattleStart)
        {
            leftClickGroup.alpha =
                Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1));

            if (Input.GetMouseButtonDown(1))
                StartBattle();
        }

        if (waitingForRightClick)
        {
            rightClickGroup.alpha =
                Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1));

            if (Input.GetMouseButtonDown(1))
            {
                rightClickUI.SetActive(false);
                waitingForRightClick = false;
                Time.timeScale = 1f;
            }
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
        rightClickUI.SetActive(true);
    }

    //시작 튜토리얼 닫기
    void CloseStartTutorial()
    {
        waitingForEnter = false;

        startPanel.SetActive(false);
        rightClickUI.SetActive(false);

        tutorialPlaying = false; 

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
        if (waitingForBattleStart) return;
        directionArrow.SetActive(false);

        StartCoroutine(FadeOutMission());

        StartCoroutine(StartBattleTutorial());
    }

    IEnumerator StartBattleTutorial()
    {
        playerController.Agent.ResetPath();
        playerController.Agent.velocity = Vector3.zero;
        playerController.enabled = false;
        playerController.Anim.SetFloat("Move", 0f);

        playerCam.Priority = 5;
        tutorialCam.Priority = 20;

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(SpawnWolves());

        yield return new WaitForSeconds(1f);

        tutorialCam.Priority = 5;
        playerCam.Priority = 20;

        yield return new WaitForSeconds(2f);

        playerController.enabled = true;
    }

    IEnumerator SpawnWolves()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Instantiate(wolfPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            yield return new WaitForSeconds(0.4f);
        }
    }

    IEnumerator TypeBattleText()
    {
        battleText.text = "";

        string msg = "이 버튼을 눌러 전투를 시작하십시오";

        foreach (char c in msg)
        {
            battleText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
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

    void StartBattle()
    {
        waitingForBattleStart = false;

        battlePanel.SetActive(false);
        leftClickUI.SetActive(false);

        tutorialPlaying = false; // 입력 허용
    }

    public void OnPlayerWeaponDraw()
    {
        if (battleTutorialShown) return; 

        battleTutorialShown = true;

        StartCoroutine(ShowBattleMission());
    }

    IEnumerator ShowBattleMission()
    {
        tutorialPlaying = true;   

        playerController.Agent.ResetPath();
        playerController.Agent.velocity = Vector3.zero;
        playerController.Anim.SetFloat("Move", 0f);

        battlePanel.SetActive(true);

        yield return StartCoroutine(TypeBattleText());

        yield return new WaitForSecondsRealtime(0.3f);

        leftClickUI.SetActive(true);
        waitingForBattleStart = true;
    }

}
