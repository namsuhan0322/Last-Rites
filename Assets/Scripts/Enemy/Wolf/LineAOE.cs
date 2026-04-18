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
            float randX = Random.Range(-width * 0.5f, width * 0.5f);
            float randZ = Random.Range(-length * 0.5f, length * 0.5f);

            Vector3 pos = transform.position +
                          transform.right * randX +
                          transform.up * randZ; 

            GameObject vfx = Instantiate(vfxPrefab, pos, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        Vector3 center = transform.position + transform.forward * (length * 0.5f);

        Actor[] actors = FindObjectsByType<Actor>(FindObjectsSortMode.None);

        foreach (var actor in actors)
        {
            if (actor == null || actor.IsDead) continue;

            if (((1 << actor.gameObject.layer) & targetLayer) == 0) continue;

            Vector3 checkPos = actor.transform.position;


            Vector3 localPos = transform.InverseTransformPoint(checkPos);
            float hitWidth = width * 0.4f;

            bool isInside =
                Mathf.Abs(localPos.x) <= hitWidth &&
                localPos.z >= 0f &&
                localPos.z <= length;

            Debug.DrawLine(transform.position, checkPos, isInside ? Color.green : Color.red, 2f);

            if (isInside)
            {
                actor.TakeDamage(damage, 1f);
            }
        }

        Destroy(gameObject);
    }
}
