using UnityEngine;

public class AvatarDebugger : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            RunDebug();
        }
    }

    // 유니티 에디터의 톱니바퀴나 점 3개 메뉴를 눌러서 게임 중 언제든 수동으로 실행할 수 있습니다.
    [ContextMenu("아바타 및 뼈대 상태 점검하기")]
    public void RunDebug()
    {
        Debug.Log("<color=cyan>=== 🕵️‍♂️ [아바타 & 뼈대 정밀 진단 시작] ===</color>");

        Animator anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("❌ [치명적 오류] 플레이어에 Animator 컴포넌트가 없습니다!");
            return;
        }

        if (!anim.enabled)
        {
            Debug.LogError("❌ [오류] Animator 컴포넌트가 꺼져(Disabled) 있습니다!");
        }

        // 1. 아바타(Avatar) 존재 여부 및 호환성 체크
        if (anim.avatar == null)
        {
            Debug.LogError("❌ [치명적 오류] Animator에 Avatar가 할당되어 있지 않습니다! (T포즈의 100% 원인)");
        }
        else
        {
            Debug.Log($"✅ [정상] 현재 할당된 아바타 이름: <color=yellow>{anim.avatar.name}</color>");
            Debug.Log($"✅ [정상] 휴머노이드(Humanoid) 여부: <color=yellow>{anim.avatar.isHuman}</color>");

            // 휴머노이드일 경우 실제 뼈대(Hips)를 추적할 수 있는지 테스트
            if (anim.avatar.isHuman)
            {
                Transform hips = anim.GetBoneTransform(HumanBodyBones.Hips);
                if (hips == null)
                {
                    Debug.LogError("❌ [뼈대 매핑 실패] 휴머노이드 아바타이지만 기준점(Hips) 뼈를 찾을 수 없습니다! 모델(Mesh)과 아바타(Avatar)가 호환되지 않습니다.");
                }
                else
                {
                    Debug.Log($"✅ [정상] 기준 뼈대(Hips) 매핑 정상 작동 중: <color=green>{hips.name}</color>");
                }
            }
        }

        // 2. 현재 화면에 켜져 있는 모델(SkinnedMeshRenderer) 상태 체크
        // false 매개변수 = 현재 꺼진 모델(대검, 쌍검 등)은 제외하고 켜진 모델(마법사)만 찾음
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>(false);

        if (smrs.Length == 0)
        {
            Debug.LogWarning("⚠️ [경고] 현재 활성화된 모델(SkinnedMeshRenderer)이 없습니다. 캐릭터가 투명 상태인가요?");
        }
        else
        {
            Debug.Log("<color=cyan>--- 현재 켜져 있는 모델 분석 ---</color>");
            foreach (SkinnedMeshRenderer smr in smrs)
            {
                if (smr.rootBone == null)
                {
                    Debug.LogError($"❌ [오류] 켜진 모델 '{smr.gameObject.name}'의 Root Bone이 비어있습니다! 애니메이션이 적용되지 않습니다.");
                }
                else
                {
                    Debug.Log($"🔍 활성화된 모델: <color=yellow>{smr.gameObject.name}</color> / 해당 모델의 Root Bone: <color=green>{smr.rootBone.name}</color>");
                }
            }
        }

        Debug.Log("<color=cyan>=== 🕵️‍♂️ [아바타 & 뼈대 정밀 진단 종료] ===</color>");
    }
}