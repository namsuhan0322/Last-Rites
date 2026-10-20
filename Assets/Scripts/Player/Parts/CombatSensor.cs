using UnityEngine;

/// <summary>
/// [역할] 주변 적 감지(반경·시야각·가림 검사) → CombatTimer로 전투 ON/OFF → 애니메이터 InCombat.
///        튜토리얼 전투 잠금이면 즉시 OFF하고 무기를 넣는다. 에디터 기즈모도 그린다.
/// [이유] 전투 감지 코드를 한곳에 모은다. 튜토리얼 값은 지금처럼 매번 실시간으로 읽는다 (R23).
/// [근거] 명세 3.3 CombatSensor, 결정 R4, R27
/// </summary>
public class CombatSensor
{
    private readonly PlayerController _owner;
    private readonly CombatTimer _combat;
    private readonly TutorialGate _tutorial;

    public CombatSensor(PlayerController owner, CombatTimer combat, TutorialGate tutorial)
    {
        _owner = owner;
        _combat = combat;
        _tutorial = tutorial;
    }

    public void Tick()
    {
        if (_tutorial.CombatLocked)
        {
            _combat.ForceOff();
            _owner.Anim.SetBool(PlayerAnimParams.InCombat, false);

            if (_owner.VisualManager != null)
                _owner.VisualManager.StowWeapon();

            return;
        }

        _combat.Update(IsEnemyInSight(), _owner.CombatCooldown, Time.deltaTime);
        _owner.Anim.SetBool(PlayerAnimParams.InCombat, _combat.InCombat);
    }

    // 반경 안 적 중 시야각(ViewAngle/2 미만) 안에 있고, Default 레이어에 가리지 않은 적이 하나라도 있으면 true
    private bool IsEnemyInSight()
    {
        Transform tr = _owner.transform;
        Collider[] targetsInViewRadius = Physics.OverlapSphere(tr.position, _owner.DetectionRadius, _owner.EnemyLayer);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - tr.position).normalized;

            if (Vector3.Angle(tr.forward, dirToTarget) < _owner.ViewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(tr.position, target.position);
                if (!Physics.Raycast(tr.position, dirToTarget, dstToTarget, LayerMask.GetMask("Default")))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// 에디터에서 선택했을 때 감지 반경(노랑)과 시야각(빨강)을 그린다. 플레이 전에도 불리므로 static.
    public static void DrawGizmos(Transform tr, float radius, float viewAngle)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(tr.position, radius);

        Vector3 viewAngleA = DirFromAngle(tr, -viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(tr, viewAngle / 2, false);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(tr.position, tr.position + viewAngleA * radius);
        Gizmos.DrawLine(tr.position, tr.position + viewAngleB * radius);
    }

    private static Vector3 DirFromAngle(Transform tr, float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += tr.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
