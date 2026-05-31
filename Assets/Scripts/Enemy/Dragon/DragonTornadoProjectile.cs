using UnityEngine;

public class DragonTornadoProjectile : MonoBehaviour
{
    private DragonBoss owner;
    private Vector3 dir;

    private float speed;
    private float lifeTime;
    private int damage;
    private LayerMask targetLayer;

    [Header("디버그 표시")]
    [SerializeField] private bool showDebugLine = true;
    [SerializeField] private float debugLineWidth = 0.2f;

    private LineRenderer line;
    private Vector3 startPos;

    public void Init(
        DragonBoss owner,
        Vector3 dir,
        float speed,
        float lifeTime,
        int damage,
        LayerMask targetLayer)
    {
        this.owner = owner;
        this.dir = dir.normalized;
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.damage = damage;
        this.targetLayer = targetLayer;

        startPos = transform.position;

        if (showDebugLine)
            CreateDebugLine();

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += dir * speed * Time.deltaTime;

        if (line != null)
        {
            line.SetPosition(0, startPos);
            line.SetPosition(1, transform.position);
        }
    }

    private void CreateDebugLine()
    {
        GameObject lineObj = new GameObject("Tornado Hit Range Line");
        line = lineObj.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.startWidth = debugLineWidth;
        line.endWidth = debugLineWidth;
        line.useWorldSpace = true;

        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position);

        Destroy(lineObj, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) == 0)
            return;

        Actor target = other.GetComponentInParent<Actor>();

        if (target == null || target == owner)
            return;

        target.TakeDamage(damage);
    }
}
    

