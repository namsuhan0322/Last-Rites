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

    [Header("공격 튜토리얼 범위")]
    public float attackMissionRange = 6f;

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

    public bool canUseSkills = false;
    bool battleTutorialShown = false;
    public bool tutorialPlaying = false;
    Coroutine missionRoutine;
    bool bossPhaseStarted = false;
    SkillTutorial skillTutorial;
    public bool canPlayerCombat = false;
    TutorialBoss spawnedBoss;
    bool checkingAttackMissionRange = false;

    [Header("스킵 기능")]
    public Transform bossFightLocation;

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

        if (checkingAttackMissionRange && !battleTutorialShown && spawnedBoss != null)
        {
            float dist = Vector3.Distance(
                playerController.transform.position,
                spawnedBoss.transform.position
            );

            if (dist <= attackMissionRange)
            {
                checkingAttackMissionRange = false;
                ShowAttackMissionFromBossRange();
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
        canPlayerCombat = false;

        playerController.Agent.ResetPath();
        playerController.Agent.velocity = Vector3.zero;
        playerController.enabled = false;
        playerController.Anim.SetFloat("Move", 0f);

        playerCam.Priority = 5;
        tutorialCam.Priority = 20;

        yield return new WaitForSeconds(2f);

        SpawnTutorialBoss();
        bossPhaseStarted = true;

        yield return new WaitForSeconds(2f);

        tutorialCam.Priority = 5;
        playerCam.Priority = 20;

        yield return new WaitForSeconds(1f);
    }

    public void ShowAttackMissionFromBossRange()
    {
        if (battleTutorialShown) return;

        battleTutorialShown = true;
        StartCoroutine(ShowBattleMission());
    }

    void SpawnTutorialBoss()
    {
        if (spawnPoints.Length > 0)
        {
            Transform spawn = spawnPoints[0];

            GameObject bossObj = Instantiate(tutorialBossPrefab, spawn.position, spawn.rotation);

            spawnedBoss = bossObj.GetComponent<TutorialBoss>();

            if (spawnedBoss != null)
            {
                spawnedBoss.SetTutorialFreeze(true);
            }

            checkingAttackMissionRange = true;
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

        TutorialBoss boss = FindFirstObjectByType<TutorialBoss>();
        boss?.SetTutorialFreeze(false);

        canPlayerCombat = true; 

        tutorialPlaying = false;
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

        TutorialBoss boss = FindFirstObjectByType<TutorialBoss>();
        boss?.SetTutorialFreeze(true);

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

        GameProgressManager.Instance.CompleteTutorial();

        // 인벤토리에 튜토리얼 보스 소울(S_000) 1개 지급
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem("S_000", 1);
            Debug.Log("튜토리얼 보스 소울(S_000) 획득!");
        }

        // 진행도와 인벤토리가 모두 변경되었으므로, 하나의 세이브 파일로 완벽하게 통합 저장
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveAllData();
        }

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

    public void SkipTutorial()
    {
        if (bossPhaseStarted) return;

        StopAllCoroutines();
        StartCoroutine(SkipCoroutine());
    }

    private IEnumerator SkipCoroutine()
    {
        EventSystemManager.Instance?.StartEvent();

        if (ScenesManager.Instance != null)
        {
            yield return StartCoroutine(ScenesManager.Instance.FadeIn());
        }

        try
        {
            if (startPanel != null) startPanel.SetActive(false);
            if (battlePanel != null) battlePanel.SetActive(false);
            if (EnterClickUI != null) EnterClickUI.SetActive(false);
            if (tutorialCompleteText != null) tutorialCompleteText.gameObject.SetActive(false);

            waitingForEnter = false;
            waitingForBattleStart = false;
            waitingForEnterClick = false;
            tutorialPlaying = false;
            bossPhaseStarted = true;

            if (tutorialCam != null) tutorialCam.Priority = 5;
            if (playerCam != null) playerCam.Priority = 20;

            if (playerController != null && bossFightLocation != null)
            {
                playerController.enabled = false;
                if (playerController.CC != null) playerController.CC.enabled = false;

                if (playerController.Agent != null && playerController.Agent.isOnNavMesh)
                {
                    playerController.Agent.Warp(bossFightLocation.position);
                }
                else
                {
                    playerController.transform.position = bossFightLocation.position;
                }

                playerController.transform.rotation = bossFightLocation.rotation;

                if (playerController.CC != null) playerController.CC.enabled = true;
                if (playerController.Agent != null) playerController.Agent.enabled = true;
                playerController.enabled = true;

                if (playerController.Anim != null) playerController.Anim.SetFloat("Move", 0f);
            }

            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.CompleteTutorial();
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem("S_000", 1);
                InventoryManager.Instance.AddItem("R_001", 99);
                InventoryManager.Instance.AddItem("P_001", 99);
                InventoryManager.Instance.AddItem("P_002", 99);
                InventoryManager.Instance.AddItem("P_003", 99);
                InventoryManager.Instance.AddCurrency(999999);
            }

            if (DataManager.Instance != null)
            {
                DataManager.Instance.SaveAllData();
            }

            if (directionArrow != null) directionArrow.SetActive(true);
            ShowMission("포탈을 통해 로비로 이동하세요");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TutorialSystem] 스킵 도중 에러가 발생했지만 강제 복구합니다.\n원인: {e.Message}\n위치: {e.StackTrace}");
        }

        yield return new WaitForSecondsRealtime(0.5f);

        if (ScenesManager.Instance != null)
        {
            yield return StartCoroutine(ScenesManager.Instance.FadeOut());
        }

        EventSystemManager.Instance?.EndEvent();
    }
}
