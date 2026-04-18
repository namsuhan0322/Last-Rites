using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TriangleMesh : MonoBehaviour
{
    public float lifeTime = 2f;
    Vector3 worldV0, worldV1, worldV2;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask targetLayer;
    private bool hasHit = false;
    [SerializeField] private GameObject vfxPrefab;
    float triWidth;
    float triLength;

    public void Init(Vector3 origin, Vector3 dir, float length, float width, float life = 2f)
    {
        lifeTime = life;
        Mesh mesh = new Mesh();
        triLength = length;
        triWidth = width;

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        Vector3 v0 = Vector3.zero;
        Vector3 v1 = forward * length + right * (width * 0.5f);
        Vector3 v2 = forward * length - right * (width * 0.5f);

        mesh.vertices = new Vector3[] { v0, v1, v2 };
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        transform.position = origin;

        worldV0 = transform.TransformPoint(v0);
        worldV1 = transform.TransformPoint(v1);
        worldV2 = transform.TransformPoint(v2);

        StartCoroutine(GrowAndExplode());
    }

    bool IsInsideTriangle(Vector3 p)
    {
        Vector3 a = worldV0;
        Vector3 b = worldV1;
        Vector3 c = worldV2;

        Vector3 v0 = c - a;
        Vector3 v1 = b - a;
        Vector3 v2 = p - a;

        float dot00 = Vector3.Dot(v0, v0);
        float dot01 = Vector3.Dot(v0, v1);
        float dot02 = Vector3.Dot(v0, v2);
        float dot11 = Vector3.Dot(v1, v1);
        float dot12 = Vector3.Dot(v1, v2);

        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
    }

    IEnumerator GrowAndExplode()
    {
        float timer = 0f;
        float duration = lifeTime;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Color color = renderer.material.color;

        color.a = 0f;
        renderer.material.color = color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            color.a = Mathf.Lerp(0f, 1f, t);
            renderer.material.color = color;

            yield return null;
        }

        int vfxCount = 30;

        for (int i = 0; i < vfxCount; i++)
        {
            Vector3 randomPos = GetRandomPointInTriangle();
            GameObject vfx = Instantiate(vfxPrefab, randomPos, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, triLength, targetLayer);

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();
            if (actor == null || actor.IsDead) continue;

            if (IsInsideTriangle(actor.transform.position))
            {
                actor.TakeDamage(damage, 1f);
            }
        }

        Destroy(gameObject);
    }

    Vector3 GetRandomPointInTriangle()
    {
        float r1 = Random.value;
        float r2 = Random.value;

        // 삼각형 내부 균일 분포
        if (r1 + r2 > 1f)
        {
            r1 = 1f - r1;
            r2 = 1f - r2;
        }

        return worldV0 + (worldV1 - worldV0) * r1 + (worldV2 - worldV0) * r2;
    }
}