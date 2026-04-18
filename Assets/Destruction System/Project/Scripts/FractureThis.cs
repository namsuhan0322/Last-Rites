using UnityEngine;
using Project.Scripts.Fractures;

public class FractureThis : MonoBehaviour
{
    [Header("오브젝트 스와핑 세팅")]
    [Tooltip("멀쩡한 상태의 원본 메쉬 (자기 자신의 MeshRenderer를 넣어도 됩니다)")]
    public GameObject originalModel;

    [Tooltip("미리 부셔둔 파편들이 모여있는 최상위 부모 객체 (Fracture)")]
    public GameObject fractureRoot;

    private void Start()
    {
        // 1. 게임 시작 시: 부서진 파편들은 숨기고, 원본만 보여줍니다.
        if (fractureRoot != null)
            fractureRoot.SetActive(false);

        // 변수가 비어있다면 자기 자신(원본 바위)을 할당합니다.
        if (originalModel == null)
            originalModel = gameObject;
    }

    // WolfBoss.cs의 돌진 패턴에서 타격 시 이 함수를 호출합니다.
    public void FractureAndDestroy()
    {
        // 2. 원본 돌멩이 숨기기 (렌더러와 콜라이더만 끕니다)
        if (originalModel != null)
        {
            MeshRenderer mr = originalModel.GetComponent<MeshRenderer>();
            if (mr) mr.enabled = false;

            Collider col = originalModel.GetComponent<Collider>();
            if (col) col.enabled = false;
        }

        // 3. 숨겨뒀던 부서진 파편들 등장!
        if (fractureRoot != null)
        {
            fractureRoot.SetActive(true);

            // 팁: 켜지자마자 파편들이 자연스럽게 무너지게 하려면
            // 보스가 부딪힌 방향으로 파편의 조인트(FixedJoint)에 물리적 충격을 주면 더 멋집니다.
            foreach (Transform chunk in fractureRoot.transform)
            {
                Rigidbody rb = chunk.GetComponent<Rigidbody>();
                ChunkNode node = chunk.GetComponent<ChunkNode>();

                if (rb != null && node != null && !node.IsStatic)
                {
                    // 파편들을 강제로 속박 해제하고 살짝 바깥으로 튕겨나가게 폭발력 추가
                    node.Unfreeze();
                    rb.AddExplosionForce(500f, transform.position, 5f);
                }
            }
        }
    }
}