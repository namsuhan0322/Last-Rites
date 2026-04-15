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
            if (TryGetComponent<MeshRenderer>(out var mr)) mr.enabled = false;
            if (TryGetComponent<Collider>(out var col)) col.enabled = false;

            gameObject.SetActive(false);

            if (graphManager != null)
            {
                int debrisLayer = LayerMask.NameToLayer("Debris");

                foreach (Transform child in graphManager.transform)
                {
                    if (debrisLayer != -1)
                    {
                        child.gameObject.layer = debrisLayer;
                    }

                    if (child.TryGetComponent<Rigidbody>(out var rb))
                    {

                        rb.AddExplosionForce(500f, transform.position, 5f);
                    }

                    Destroy(child.gameObject, 5f);
                }

                Destroy(graphManager.gameObject, 6f);
            }
        }
    }
}