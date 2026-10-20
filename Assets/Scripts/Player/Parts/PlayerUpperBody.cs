using UnityEngine;

/// <summary>
/// [역할] 상체 레이어(레이어 1) 가중치: 몸 전체를 쓰는 상태면 즉시 끄고, 전투 중이거나 칼을 뽑고·넣는 중이면 서서히 켠다.
///        칼 뽑기/넣기 애니메이션 이벤트가 캔슬돼 놓쳤을 때를 대비해 무기 외형을 맞춘다.
/// [이유] 컨트롤러의 ManageUpperBodyWeight를 떼어 낸다. 상태 6개 나열은 상태 속성 IsFullBodyAction으로 대신한다.
/// [근거] 명세 3.4 PlayerUpperBody, 결정 R29, R30
/// </summary>
public class PlayerUpperBody
{
    private const float BlendSpeed = 10f;   // 가중치를 Time.deltaTime × 10 비율로 목표값에 가깝게 (지금 값)

    private readonly PlayerController _owner;

    public PlayerUpperBody(PlayerController owner)
    {
        _owner = owner;
    }

    public void Tick()
    {
        Animator anim = _owner.Anim;
        PlayerState state = _owner.StateMachine.CurrentState;

        // 전신을 써야 하는 상태(공격, 회피 등)에서는 상체 레이어를 즉시 끈다
        if (state != null && state.IsFullBodyAction)
        {
            anim.SetLayerWeight(PlayerAnimParams.UpperBodyLayer, 0f);
            return;
        }

        AnimatorStateInfo upperState = anim.GetCurrentAnimatorStateInfo(PlayerAnimParams.UpperBodyLayer);

        bool isEmpty = upperState.IsName(PlayerAnimParams.StateUpperEmpty);
        // 변수 이름과 애니 이름의 뜻이 서로 바뀌어 있지만(SheatheHips = 넣기), 둘 다 '레이어를 켜는 중'으로만 쓰여 결과는 같다 (지금 코드 그대로)
        bool isDrawing = upperState.IsName(PlayerAnimParams.StateUpperSheathe);
        bool isStowing = upperState.IsName(PlayerAnimParams.StateUpperUnsheathe);

        // 이벤트 캔슬 대비용 안전장치
        if (isEmpty && !_owner.InCombat)
        {
            _owner.OnWeaponStowed();
        }
        else if (upperState.IsName(PlayerAnimParams.StateUpperCombat) && _owner.InCombat)
        {
            _owner.OnWeaponDrawn();
        }

        // 전투 중이거나, 칼 뽑기/넣기 중이면 레이어 켜기, 아니면 끄기
        float targetWeight = (_owner.InCombat || isDrawing || isStowing) ? 1f : 0f;
        float current = anim.GetLayerWeight(PlayerAnimParams.UpperBodyLayer);
        anim.SetLayerWeight(PlayerAnimParams.UpperBodyLayer, Mathf.Lerp(current, targetWeight, Time.deltaTime * BlendSpeed));
    }
}
