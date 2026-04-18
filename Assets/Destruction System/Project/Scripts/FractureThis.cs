using UnityEngine;
using Project.Scripts.Fractures;

public class FractureThis : MonoBehaviour
{
    [Header("오브젝트 스와핑 세팅")]
    public GameObject originalModel;
    public GameObject fractureRoot;
    private GameObject _backupFractureRoot;

    [Header("파편 관리")]
    [Tooltip("파편이 몇 초 뒤에 사라질지 설정합니다.")]
    public float shardLifeTime = 7f;

    private void Start()
    {
        if (fractureRoot != null)
        {
            // 자식으로 넣지 않고 원본의 부모 계층에 복사
            _backupFractureRoot = Instantiate(fractureRoot, fractureRoot.transform.parent);

            _backupFractureRoot.transform.position = fractureRoot.transform.position;
            _backupFractureRoot.transform.rotation = fractureRoot.transform.rotation;
            _backupFractureRoot.transform.localScale = fractureRoot.transform.localScale;

            _backupFractureRoot.SetActive(false);
            fractureRoot.SetActive(false);
        }

        if (originalModel == null)
            originalModel = gameObject;
    }

    public void FractureAndDestroy()
    {
        if (originalModel != null)
        {
            MeshRenderer mr = originalModel.GetComponent<MeshRenderer>();
            if (mr) mr.enabled = false;

            Collider col = originalModel.GetComponent<Collider>();
            if (col) col.enabled = false;
        }

        if (fractureRoot != null)
        {
            fractureRoot.SetActive(true);

            foreach (Transform chunk in fractureRoot.transform)
            {
                Rigidbody rb = chunk.GetComponent<Rigidbody>();
                ChunkNode node = chunk.GetComponent<ChunkNode>();

                if (rb != null && node != null && !node.IsStatic)
                {
                    node.Unfreeze();
                    rb.AddExplosionForce(500f, transform.position, 5f);
                }
            }

            // ==========================================
            // [핵심 추가] 설정된 시간(shardLifeTime) 후에 파편 덩어리를 삭제합니다.
            // ==========================================
            Destroy(fractureRoot, shardLifeTime);
        }
    }

    public void ResetFracture()
    {
        if (originalModel != null)
        {
            MeshRenderer mr = originalModel.GetComponent<MeshRenderer>();
            if (mr) mr.enabled = true;

            Collider col = originalModel.GetComponent<Collider>();
            if (col) col.enabled = true;
        }

        // 리스폰 시 이미 부서져 있는 파편이 있다면 즉시 제거 (중복 방지)
        if (fractureRoot != null)
        {
            Destroy(fractureRoot);
        }

        if (_backupFractureRoot != null)
        {
            fractureRoot = Instantiate(_backupFractureRoot, _backupFractureRoot.transform.parent);

            fractureRoot.transform.position = _backupFractureRoot.transform.position;
            fractureRoot.transform.rotation = _backupFractureRoot.transform.rotation;
            fractureRoot.transform.localScale = _backupFractureRoot.transform.localScale;

            fractureRoot.SetActive(false);
        }
    }
}