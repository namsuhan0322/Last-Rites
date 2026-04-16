using UnityEngine;
using System.Collections;

public class LineAOE : MonoBehaviour
{
    public float lifeTime = 3f;
    public float width = 2f;
    public float length = 15f;
    public int damage = 20;
    public LayerMask targetLayer;

    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private int vfxCount = 15;

    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;

    public void Init(float time)
    {
        lifeTime = time;

        transform.localScale = new Vector3(width, length, 1f);

        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();

        if (meshRenderer != null)
        {
            Color c = meshRenderer.material.color;
            c.a = 0f;
            meshRenderer.material.color = c;
        }

        StartCoroutine(Process());
    }

    IEnumerator Process()
    {
        float timer = 0f;

        Vector3 halfExtents = new Vector3(width * 0.5f, 1f, length * 0.5f);

        while (timer < lifeTime)
        {
            timer += Time.deltaTime;
            float t = timer / lifeTime;

            if (meshRenderer != null)
            {
                Color c = meshRenderer.material.color;
                c.a = Mathf.Lerp(0f, 1f, t);
                meshRenderer.material.color = c;
            }

            yield return null;
        }

        for (int i = 0; i < vfxCount; i++)
        {
            Vector3 randomPos = GetRandomPointInBox();
            GameObject vfx = Instantiate(vfxPrefab, randomPos, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        Collider[] hits = Physics.OverlapBox(
            transform.position,
            halfExtents,
            transform.rotation * Quaternion.Euler(-90f, 0f, 0f),
            targetLayer
        );

        foreach (var hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();
            if (actor == null || actor.IsDead) continue;

            actor.TakeDamage(damage, 1f);
        }

        Destroy(gameObject);
    }

    Vector3 GetRandomPointInBox()
    {
        Vector3 local = new Vector3(
            Random.Range(-width * 0.5f, width * 0.5f),
            0f,
            Random.Range(-length * 0.5f, length * 0.5f)
        );

        return transform.TransformPoint(local);
    }
}
