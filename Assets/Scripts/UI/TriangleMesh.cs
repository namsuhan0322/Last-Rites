using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TriangleMesh : MonoBehaviour
{
    public float lifeTime = 2f;
    Vector3 worldV0, worldV1, worldV2;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask targetLayer;
    private bool hasHit = false;

    void Update()
    {
        if (hasHit) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, 10f, targetLayer);

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();
            if (actor == null || actor.IsDead) continue;

            if (IsInsideTriangle(actor.transform.position))
            {
                actor.TakeDamage(damage, 1f);
                hasHit = true; 
                break;
            }
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Init(Vector3 origin, Vector3 dir, float length, float width)
    {
        Mesh mesh = new Mesh();

        dir.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, dir);

        Vector3 v0 = Vector3.zero;
        Vector3 v1 = dir * length + right * (width * 0.5f);
        Vector3 v2 = dir * length - right * (width * 0.5f);

        Vector3[] vertices = new Vector3[] { v0, v1, v2 };
        int[] triangles = new int[] { 0, 1, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        transform.position = origin;

        worldV0 = transform.TransformPoint(v0);
        worldV1 = transform.TransformPoint(v1);
        worldV2 = transform.TransformPoint(v2);
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
}