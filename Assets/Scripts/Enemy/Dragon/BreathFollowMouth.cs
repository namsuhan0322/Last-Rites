using UnityEngine;

public class BreathFollowMouth : MonoBehaviour
{
    private Transform mouth;
    private Vector3 rotationOffset;

    public void Init(Transform mouth, Transform bossRoot, Vector3 rotationOffset)
    {
        this.mouth = mouth;
        this.rotationOffset = rotationOffset;
    }

    private void LateUpdate()
    {
        if (mouth == null)
            return;

        // 위치는 입 따라감
        transform.position = mouth.position;

        // 입의 좌우 방향만 따라감, 위아래 기울기는 제거
        Vector3 dir = mouth.forward;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            dir = mouth.parent.forward;

        dir.Normalize();

        transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(rotationOffset);
    }
}