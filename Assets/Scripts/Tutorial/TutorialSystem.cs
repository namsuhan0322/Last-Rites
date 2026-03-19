using System.Collections;
using UnityEngine;
using TMPro;
using Cinemachine;

public class TutorialSystem : MonoBehaviour
{
    [Header("시작 판넬")]
    public GameObject startPanel;
    public TMP_Text startText;
    public GameObject EnterClickUI;   // 마우스 우클릭 아이콘
    public CanvasGroup EnterClickGroup;

    [Header("미션UI")]
    public TMP_Text missionText;

    [Header("전투 튜토리얼")]
    public GameObject battlePanel;
    public TMP_Text battleText;
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera tutorialCam;

    [Header("튜토리얼 보스")]
    public GameObject tutorialBossPrefab;

    public Transform[] spawnPoints;    // 늑대 3마리 위치

    [Header("튜토리얼 완료 UI")]
    public TMP_Text tutorialCompleteText;


    public PlayerController playerController; // 추가
    public GameObject wolfPrefab;

    public Camera mainCamera;

    bool waitingForBattleStart = false;


    public float typingSpeed = 0.05f;
    public float blinkSpeed = 2f;

    public bool waitingForEnterClick = false;

    string startMessage = "마우스 우클릭으로 이동하십시오";
    string goalMessage = "목표 지점으로 이동하세요";

    bool waitingForEnter = false;
    public GameObject directionArrow;


    bool battleTutorialShown = false;
    public bool tutorialPlaying = false;
    Coroutine missionRoutine;
    bool bossPhaseStarted = false;
    SkillTutorial skillTutorial;

    void Start()
    {
        tutorialPlaying = true;
        directionArrow.SetActive(true);
        startPanel.SetActive(true);
        missionText.gameObject.SetActive(false);
        EnterClickUI.SetActive(false);
        StartCoroutine(TypeStartText());
        skillTutorial = FindFirstObjectByType<SkillTutorial>();
    }

    void Update()
    {
        if (waitingForEnter)
        {
            EnterClickGroup.alpha =
                Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1));

            if (Input.GetKeyDown(KeyCode.Return))
                CloseStartTutorial();
        }

        if (waitingForBattleStart)
        {
            EnterClickGroup.alpha =
                Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1));

            if (Input.GetKeyDown(KeyCode.Return))
                StartBattle();
        }

        if (waitingForEnterClick)
        {
            EnterClickGroup.alpha =
                Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1));

            if (Input.GetKeyDown(KeyCode.Return))
            {
                EnterClickUI.SetActive(false);
                waitingForEnterClick = false;
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
        EnterClickUI.SetActive(true);
    }

    //시작 튜토리얼 닫기
    void CloseStartTutorial()
    {
        waitingForEnter = false;

        startPanel.SetActive(false);
        EnterClickUI.SetActive(false);

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
        string msg = "<sprite=0> 버튼을 눌러 전투를 시작하십시오";

        battleText.text = msg;

        string afterSprite = " 버튼을 눌러 전투를 시작하십시오";
        battleText.text = "<sprite=0>"; 

        foreach (char c in afterSprite)
        {
            battleText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

    //미션 사라지게 하기
    public IEnumerator FadeOutMission()
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
        EnterClickUI.SetActive(false);

        tutorialPlaying = false; // 입력 허용

        playerController.enabled = true;
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

        playerController.enabled = false;

        playerController.Agent.ResetPath();
        playerController.Agent.velocity = Vector3.zero;
        playerController.Anim.SetFloat("Move", 0f);

        battlePanel.SetActive(true);

        yield return StartCoroutine(TypeBattleText());

        yield return new WaitForSecondsRealtime(0.3f);
        EnterClickUI.SetActive(true);
        waitingForBattleStart = true;
    }

    public void ShowMission(string msg)
    {
        missionText.gameObject.SetActive(true);

        Color c = missionText.color;
        c.a = 1f;
        missionText.color = c;

        if (missionRoutine != null)
            StopCoroutine(missionRoutine);

        missionRoutine = StartCoroutine(TypeMission(msg));
    }

    IEnumerator TypeMission(string message)
    {
        missionText.text = "";

        foreach (char c in message)
        {
            missionText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

    //보스 스타트
    public void StartBossPhase()
    {
        if (bossPhaseStarted) return;

        bossPhaseStarted = true;

        StartCoroutine(BossPhaseRoutine());
    }

    //보스 인트로
    IEnumerator BossPhaseRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        TutorialMinion[] minions = FindObjectsByType<TutorialMinion>(FindObjectsSortMode.None);

        foreach (var m in minions)
        {
            m.KillMinion();
        }

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(FadeOutMission());

        playerController.Agent.ResetPath();
        playerController.Agent.velocity = Vector3.zero;
        playerController.enabled = false;
        playerController.Anim.SetFloat("Move", 0f);

        playerCam.Priority = 5;
        tutorialCam.Priority = 20;

        yield return new WaitForSeconds(1f);

        if (spawnPoints.Length > 0)
        {
            Transform spawn = spawnPoints[1];
            Instantiate(tutorialBossPrefab, spawn.position, spawn.rotation);
        }

        yield return new WaitForSeconds(2.5f);

        tutorialCam.Priority = 5;
        playerCam.Priority = 20;

        yield return new WaitForSeconds(1f);

        playerController.enabled = true;
    }

    //보스끝인트로
    public void EndBossIntro()
    {
        tutorialCam.Priority = 5;
        playerCam.Priority = 20;

        playerController.enabled = true;
    }

    //튜토리얼 완료 보여주기
    public void ShowTutorialComplete()
    {
        StartCoroutine(TutorialCompleteRoutine());
    }


    //튜토리얼 완료
    IEnumerator TutorialCompleteRoutine()
    {
        yield return new WaitForSeconds(1f);

        float t = 0f;

        if (skillTutorial != null)
            skillTutorial.ShowTutorialCompleteOverlay();

        tutorialCompleteText.gameObject.SetActive(true);

        tutorialCompleteText.text = "튜토리얼 완료";

        GameProgressManager.Instance.progressData.isTutorialCleared = true;
        GameProgressManager.Instance.SaveProgress(); // 즉시 파일로 저장

        t = 0f;
        Color c = tutorialCompleteText.color;
        tutorialCompleteText.transform.localScale = Vector3.one * 0.8f;

        while (t < 1f)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(0, 1, t);
            tutorialCompleteText.color = c;

            tutorialCompleteText.transform.localScale = Vector3.Lerp(
                Vector3.one * 0.8f,
                Vector3.one,
                t
            );

            yield return null;
        }
    }
}
