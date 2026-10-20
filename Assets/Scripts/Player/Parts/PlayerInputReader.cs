using UnityEngine;

/// <summary>
/// [역할] 플레이어 입력 8종을 이름 있는 속성으로 읽는다. 부를 때마다 실제 입력을 바로 읽는다(미리 저장하지 않음).
/// [이유] Input.GetMouseButton(1) 같은 호출이 컨트롤러와 상태 5개에 흩어져 있어 키 배치를 한눈에 볼 수 없었다.
///        실시간으로 읽으므로 한 프레임에 여러 번 읽어도 지금과 결과가 같다.
/// [근거] 명세 3.4 PlayerInputReader (AnySkillPressed는 조작감 선입력으로 쓰는 곳이 없어져 삭제)
/// </summary>
public static class PlayerInputReader
{
    public static bool MoveHeld => Input.GetMouseButton(1);          // 우클릭 누르고 있음 → 이동
    public static bool MovePressed => Input.GetMouseButtonDown(1);   // 우클릭 누른 순간
    public static bool AttackHeld => Input.GetMouseButton(0);        // 좌클릭 누르고 있음
    public static bool AttackPressed => Input.GetMouseButtonDown(0); // 좌클릭 누른 순간 → 공격
    public static bool DashPressed => Input.GetKeyDown(KeyCode.Space);
    public static bool PotionPressed => Input.GetKeyDown(KeyCode.Alpha1);
    public static Vector3 MousePosition => Input.mousePosition;

    public static bool SkillPressed(SkillSlot slot) => Input.GetKeyDown(SkillKey(slot));

    public static KeyCode SkillKey(SkillSlot slot)
    {
        switch (slot)
        {
            case SkillSlot.Q: return KeyCode.Q;
            case SkillSlot.W: return KeyCode.W;
            case SkillSlot.E: return KeyCode.E;
            case SkillSlot.R: return KeyCode.R;
            default: return KeyCode.V;
        }
    }
}
