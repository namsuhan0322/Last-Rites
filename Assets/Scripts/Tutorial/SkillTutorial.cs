using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillTutorial : MonoBehaviour
{
    public Image QSkill;
    public Image WSkill;
    public Image ESkill;
    public Image RSkill;
    public Image VSkill;
    public Image grayOverlay;
    public RectTransform qHighlight;
    bool stunTutorialPlaying = false;
    public GameObject battlePanel;
    public TMP_Text battleText;

    public Color normalColor = Color.white;
    public Color grayColor = new Color(0.3f, 0.3f, 0.3f, 1);
    public TutorialSystem tutorialSystem;
    bool playing = false;
    bool alreadyTriggered = false;
    public Cinemachine.CinemachineVirtualCamera playerCam;
    bool firstQUsed = false;
    bool stunTutorialShown = false;
    float defaultFOV;
    bool dodgeTutorialPlaying = false;
    bool dodgeTutorialShown = false;

    public RectTransform staminaHighlight;

    bool staminaTutorialPlaying = false;
    bool staminaTutorialShown = false;
    Vector3 staminaBaseScale;
    void Start()
    {
        defaultFOV = playerCam.m_Lens.FieldOfView;
        staminaBaseScale = staminaHighlight.localScale;
    }

    void Awake()
    {
        if (tutorialSystem == null)
            tutorialSystem = FindFirstObjectByType<TutorialSystem>();
    }

    void Update()
    {
        if (!playing && !stunTutorialPlaying && !dodgeTutorialPlaying && !staminaTutorialPlaying) return;

        if (playing && Input.GetKeyDown(KeyCode.Q))
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();

            if (pc.TryUseSkill(KeyCode.Q, "Skill_Q", 20, 5f))
            {
                pc.Anim.SetTrigger("Skill_Q");

                firstQUsed = true;
                EndTutorial();
            }
        }

        float scale = 1 + Mathf.Sin(Time.unscaledTime * 5f) * 0.12f;
        qHighlight.localScale = Vector3.one * scale;

        if (staminaTutorialPlaying)
        {
            staminaHighlight.localScale = staminaBaseScale * scale;
        }

        if (stunTutorialPlaying && Input.GetKeyDown(KeyCode.Return))
        {
            EndStunTutorial();
        }

        if (dodgeTutorialPlaying && Input.GetKeyDown(KeyCode.Space))
        {
            EndBossDodgeTutorial();
        }

        if (staminaTutorialPlaying && Input.GetKeyDown(KeyCode.Return))
        {
            EndStaminaTutorial();
        }
    }


    //튜토리얼 시작
    public void StartTutorial()
    {
        playing = true;

        StartCoroutine(FadeGray());
        StartCoroutine(ZoomCamera());

        battlePanel.SetActive(true);
        battleText.text = "Q를 눌러 스킬을 쓰시오";

        tutorialSystem.EnterClickUI.SetActive(false);
        tutorialSystem.waitingForEnterClick = false;

        WSkill.color = grayColor;
        ESkill.color = grayColor;
        RSkill.color = grayColor;
        VSkill.color = grayColor;

        QSkill.color = normalColor;

        qHighlight.gameObject.SetActive(true);
    }


    //튜토리얼 끝내기
    void EndTutorial()
    {
        playing = false;

        Time.timeScale = 1f;

        battlePanel.SetActive(false);

        tutorialSystem.EnterClickUI.SetActive(false);
        tutorialSystem.waitingForEnterClick = false;

        StartCoroutine(FadeOutGray());
        StartCoroutine(ResetCamera());

        WSkill.color = normalColor;
        ESkill.color = normalColor;
        RSkill.color = normalColor;
        VSkill.color = normalColor;

        qHighlight.gameObject.SetActive(false);
    }

    //적이 맞았나?
    public void OnFirstHitEnemy()
    {
        if (alreadyTriggered) return;

        alreadyTriggered = true;

        StartCoroutine(StartSkillTutorialDelayed());
    }


    //딜레이
    IEnumerator StartSkillTutorialDelayed()
    {
        yield return new WaitForSeconds(1f);

        StartCoroutine(SlowMotionBeforeTutorial());
    }

    IEnumerator SlowMotionBeforeTutorial()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.07f);

        Time.timeScale = 0.15f; 
        yield return new WaitForSecondsRealtime(1.0f); 

        Time.timeScale = 0f;

        StartTutorial();
    }

    //회색빛 및 돌아가기
    IEnumerator FadeGray()
    {
        float t = 0;
        Color c = grayOverlay.color;

        while (t < 1)
        {
            t += Time.unscaledDeltaTime * 2f;
            c.a = Mathf.Lerp(0, 0.45f, t);
            grayOverlay.color = c;
            yield return null;
        }
    }

    IEnumerator FadeOutGray()
    {
        float t = 0;
        Color c = grayOverlay.color;

        while (t < 1)
        {
            t += Time.unscaledDeltaTime * 2f;
            c.a = Mathf.Lerp(0.45f, 0f, t);
            grayOverlay.color = c;
            yield return null;
        }
    }

    //줌인 줌아웃

    IEnumerator ZoomCamera()
    {
        float start = playerCam.m_Lens.FieldOfView;
        float target = defaultFOV - 10f;

        float t = 0;

        while (t < 1)
        {
            t += Time.unscaledDeltaTime * 3f;
            playerCam.m_Lens.FieldOfView = Mathf.Lerp(start, target, t);
            yield return null;
        }
    }

    IEnumerator ResetCamera()
    {
        float start = playerCam.m_Lens.FieldOfView;
        float target = defaultFOV;

        float t = 0;

        while (t < 1)
        {
            t += Time.unscaledDeltaTime * 3f;
            playerCam.m_Lens.FieldOfView = Mathf.Lerp(start, target, t);
            yield return null;
        }
    }

    public void OnEnemyStunned()
    {
        if (stunTutorialPlaying || stunTutorialShown) return;

        stunTutorialPlaying = true;
        playing = true;   

        StartCoroutine(StunTutorial());
    }
    IEnumerator StunTutorial()
    {
        Time.timeScale = 0.15f;
        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 0f;

        StartCoroutine(FadeGray());
        StartCoroutine(ZoomCamera());

        battlePanel.SetActive(true);
        battleText.text = "일정 피해를 누적시키면 기절합니다.";

        tutorialSystem.EnterClickUI.SetActive(true);
        tutorialSystem.waitingForEnterClick = true;
    }

    void EndStunTutorial()
    {
        stunTutorialPlaying = false;
        playing = false;  

        Time.timeScale = 1f;

        battlePanel.SetActive(false);
        tutorialSystem.EnterClickUI.SetActive(false);

        StartCoroutine(FadeOutGray());
        StartCoroutine(ResetCamera());

        stunTutorialShown = true;

        tutorialSystem.ShowMission("스킬을 사용해 피해량을\n누적시키시오");
    }

    public void OnPlayerUsedQSkill()
    {
        if (!playing) return;

        if (firstQUsed) return;

        firstQUsed = true;

        EndTutorial();
    }

    //오버레이
    public void ShowTutorialCompleteOverlay()
    {
        StartCoroutine(FadeGray());
    }


    //회피 스킬 튜토리얼
    public void StartBossDodgeTutorial()
    {
        if (dodgeTutorialShown) return;
        dodgeTutorialShown = true;

        if (dodgeTutorialPlaying) return;

        dodgeTutorialPlaying = true;

        Time.timeScale = 0f;

        StartCoroutine(FadeGray());
        StartCoroutine(ZoomCamera());

        battlePanel.SetActive(true);
        battleText.text = "보스가 강력한 일격을 준비합니다\nSpace를 이용해 피하십시오";

        tutorialSystem.EnterClickUI.SetActive(false);
    }

    void EndBossDodgeTutorial()
    {
        dodgeTutorialPlaying = false;

        Time.timeScale = 1f;

        battlePanel.SetActive(false);

        StartCoroutine(FadeOutGray());
        StartCoroutine(ResetCamera());

        StartCoroutine(StartStaminaTutorialDelay());
    }

    IEnumerator StartStaminaTutorialDelay()
    {
        if (staminaTutorialShown) yield break;

        yield return new WaitForSeconds(1f);

        staminaTutorialShown = true;

        StartStaminaTutorial();
    }

    void StartStaminaTutorial()
    {
        staminaTutorialPlaying = true;

        Time.timeScale = 0f;

        StartCoroutine(FadeGray());
        StartCoroutine(ZoomCamera());

        staminaHighlight.gameObject.SetActive(true);

        battlePanel.SetActive(true);
        battleText.text = "회피 시 일정 스테미나가 닳습니다";

        tutorialSystem.EnterClickUI.SetActive(true);
        tutorialSystem.waitingForEnterClick = true;
    }

    void EndStaminaTutorial()
    {
        staminaTutorialPlaying = false;

        Time.timeScale = 1f;

        battlePanel.SetActive(false);
        battleText.text = "";

        staminaHighlight.gameObject.SetActive(false);

        tutorialSystem.EnterClickUI.SetActive(false);
        tutorialSystem.waitingForEnterClick = false;

        StartCoroutine(FadeOutGray());
        StartCoroutine(ResetCamera());
    }
}
