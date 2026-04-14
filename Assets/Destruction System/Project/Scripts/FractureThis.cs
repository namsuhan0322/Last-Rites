using UnityEngine;
using Random = System.Random;

namespace Project.Scripts.Fractures
{
    public class FractureThis : MonoBehaviour
    {
        [SerializeField] private Anchor anchor = Anchor.Bottom;
        [SerializeField] private int chunks = 500;
        [SerializeField] private float density = 50;
        [SerializeField] private float internalStrength = 100;
            
        [SerializeField] private Material insideMaterial;
        [SerializeField] private Material outsideMaterial;

        private Random rng = new Random();

        private void Start()
        {
            //FractureGameobject();
            //gameObject.SetActive(false);
        }

        public ChunkGraphManager FractureGameobject()
        {
            var seed = rng.Next();
            return Fracture.FractureGameObject(
                gameObject,
                anchor,
                seed,
                chunks,
                insideMaterial,
                outsideMaterial,
                internalStrength,
                density
            );
        }

        public void FractureAndDestroy()
        {
            Debug.Log($"{gameObject.name} 파괴 시퀀스 시작");

            // 1. 파괴 실행
            var graphManager = FractureGameobject();

            // 2. 원본을 즉시 완전히 숨기고 물리 연산에서 제외
            // 단순히 SetActive(false)만 하면 연산 타이밍 때문에 잔상이 남을 수 있음
            if (TryGetComponent<MeshRenderer>(out var mr)) mr.enabled = false;
            if (TryGetComponent<Collider>(out var col)) col.enabled = false;

            // 최종적으로 오브젝트 비활성화
            gameObject.SetActive(false);

            // 3. 파편들 삭제 처리
            if (graphManager != null)
            {
                foreach (Transform child in graphManager.transform)
                {
                    Destroy(child.gameObject, 5f);
                }
                Destroy(graphManager.gameObject, 6f);
            }
        }
    }
}