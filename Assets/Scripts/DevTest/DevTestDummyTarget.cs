using System;
using UnityEngine;

/// <summary>
/// [역할] 테스트 씬에서 가만히 서 있는 연습용 적. 받은 데미지와 남은 HP를 머리 위에 보여 주고,
///        맞을 때마다 '누가 데미지를 줬는지'(히트박스·마법탄·범위 공격 등)를 함께 알린다.
/// [이유] 리팩토링 전후로 플레이어가 주는 데미지 숫자가 같은지, 어느 경로로 들어왔는지 비교하기 위해.
///        마법사 Q처럼 한 번에 두 경로로 맞는 경우를 구분해야 한다 (검증 기록 U8).
/// [근거] 명세 3.6-3, 결정 R19, R26(데미지 기록 화면+Console, 사용자 결정 C), 공격 판정 H9(약점 경유 표시)
/// </summary>
public class DevTestDummyTarget : Actor
{
    /// 어떤 더미든 데미지를 받으면 알린다: (더미 이름, 받은 데미지, 출처). 화면·Console 기록(DevTestPlayerOverlay)용.
    public static event Action<string, int, string> OnAnyDamaged;

    [Tooltip("시작 HP. 테스트 중 쉽게 죽지 않도록 크게 잡는다.")]
    [SerializeField] private int _startHP = 100000;

    [Tooltip("죽은 뒤 다시 살아나기까지 걸리는 시간(초)")]
    [SerializeField] private float _reviveDelay = 1f;

    private Vector3 _homePosition;
    private int _lastDamage;
    private float _reviveAt = -1f;

    protected override void Start()
    {
        base.Start();
        _homePosition = transform.position;
        InitActor(_startHP);
    }

    protected override void Update()
    {
        base.Update();
        if (_reviveAt > 0f && Time.time >= _reviveAt)
        {
            _reviveAt = -1f;
            InitActor(_startHP);
        }
    }

    public override void TakeDamage(int damage, float severityOverride = -1f, bool isHeavyAttack = false, bool showDamageText = true)
    {
        // 판정은 Actor 그대로 두고, 들어온 값만 기록한다 (플레이어 쪽에서 계산한 최종 데미지 = 이 값)
        if (!IsDead)
        {
            _lastDamage = damage;
            OnAnyDamaged?.Invoke(name, damage, FindDamageSource());
        }
        base.TakeDamage(damage, severityOverride, isHeavyAttack, showDamageText);
    }

    /// 호출 기록(스택)을 거슬러 올라가 데미지를 준 스크립트를 찾는다. 게임 코드는 건드리지 않기 위해 이 방법을 쓴다 (결정 R1).
    /// 에디터 테스트 전용: 빌드(IL2CPP)에서는 호출 기록이 부정확할 수 있다.
    private static string FindDamageSource()
    {
        var trace = new System.Diagnostics.StackTrace(2, false);   // 이 함수와 TakeDamage는 건너뜀
        bool viaWeakPoint = false;                                // 약점이 보스로 넘긴 데미지인지 (공격 판정 H9)
        for (int i = 0; i < trace.FrameCount; i++)
        {
            Type type = trace.GetFrame(i)?.GetMethod()?.DeclaringType;
            // 코루틴·람다는 컴파일러가 만든 중첩 클래스(<...>)라서 바깥 클래스로 올라간다 (예: AoESkillEffect의 데미지 코루틴)
            while (type != null && type.DeclaringType != null && type.Name.StartsWith("<")) type = type.DeclaringType;
            if (type == null) continue;

            if (type == typeof(WeakPoint)) { viaWeakPoint = true; continue; }

            string source = null;
            if (type == typeof(WeaponHitbox)) source = "히트박스";
            else if (type == typeof(VFXProjectile)) source = "마법탄";
            else if (type == typeof(Projectile)) source = "창 버프 투사체";
            else if (type == typeof(AoESkillEffect)) source = "범위 공격";
            else if (type == typeof(LaserSkillEffect)) source = "레이저";
            else if (type == typeof(BlackholeSkillEffect)) source = "블랙홀";

            if (source != null) return viaWeakPoint ? source + " (약점 경유)" : source;
        }
        return viaWeakPoint ? "기타 (약점 경유)" : "기타";
    }

    protected override void Die()
    {
        base.Die();
        _reviveAt = Time.time + _reviveDelay;
    }

    /// 위치와 HP를 처음 상태로 되돌린다 (블랙홀에 끌려간 뒤 등).
    public void ResetDummy()
    {
        transform.position = _homePosition;
        _reviveAt = -1f;
        InitActor(_startHP);
    }

    private void OnGUI()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        Vector3 screen = cam.WorldToScreenPoint(transform.position + Vector3.up * 2.4f);
        if (screen.z < 0f) return;
        GUI.Box(new Rect(screen.x - 80f, Screen.height - screen.y, 160f, 40f),
                $"{name}\nHP {CurrentHP}  마지막 -{_lastDamage}");
    }
}
