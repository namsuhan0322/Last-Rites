using UnityEngine;

[ExecuteInEditMode]
public class BoneSyncer : MonoBehaviour
{
    [Header("정상적으로 작동하는 원본 메쉬")]
    public SkinnedMeshRenderer targetRenderer;

    [ContextMenu("뼈대 동기화 실행! (여기 우클릭)")]
    public void SyncBones()
    {
        SkinnedMeshRenderer myRenderer = GetComponent<SkinnedMeshRenderer>();

        if (targetRenderer == null || myRenderer == null)
        {
            Debug.LogError("타겟 렌더러나 내 렌더러가 없습니다!");
            return;
        }

        myRenderer.bones = targetRenderer.bones;
        myRenderer.rootBone = targetRenderer.rootBone;

        Debug.Log($"<color=cyan>[{gameObject.name}] 뼈대 동기화 완료! 이제 원본 뼈대를 지워도 됩니다.</color>");
    }
}