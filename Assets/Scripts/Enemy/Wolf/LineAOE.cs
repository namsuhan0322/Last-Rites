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

            Collider col = actor.GetComponentInChildren<Collider>();
            if (col == null) continue;

            Vector3 checkPos = col.bounds.center;
          
            float backOffset = 30f; 

            Vector3 origin = transform.position - transform.up * backOffset; 

            Vector3 toTarget = checkPos - origin;

            Vector3 forward = transform.up;
            Vector3 right = transform.right;

            float z = Vector3.Dot(toTarget, forward);
            float x = Vector3.Dot(toTarget, right);

            float hitWidth = width * 0.45f;
            float radius = col.bounds.extents.x;

            bool isInside =
                Mathf.Abs(x) <= (hitWidth + radius * 0.5f) &&
                z >= 0f &&          
                z <= length;        

            Debug.DrawRay(origin, forward * length, Color.blue, 2f);
            Debug.DrawRay(origin, right * width, Color.yellow, 2f);
            if (isInside)
            {
                actor.TakeDamage(damage, 1f);
            }
        }

        Destroy(gameObject);
    }
}
