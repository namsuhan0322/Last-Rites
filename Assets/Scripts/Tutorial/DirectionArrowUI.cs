using UnityEngine;
using TMPro;

public class DirectionArrowUI : MonoBehaviour
{
    public Transform player;
    public Transform goal;

    public RectTransform arrowUI;

    public float screenOffset = 80f;
    public float hideDistance = 2f;

    void Update()
    {
        if (player == null || goal == null) return;

        Vector3 dir = goal.position - player.position;
        dir.y = 0;

        float dist = dir.magnitude;

        // 목표 가까우면 화살표 숨김
        if (dist < hideDistance)
        {
            arrowUI.gameObject.SetActive(false);
            return;
        }

        arrowUI.gameObject.SetActive(true);

        dir.Normalize();

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        // 화살표 회전
        arrowUI.rotation = Quaternion.Euler(0, 0, -angle + 90f);

        // 화면 중앙 기준
        Vector3 center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        // 항상 같은 거리 유지
        Vector2 offset = new Vector2(dir.x, dir.z) * screenOffset;

        arrowUI.position = center + new Vector3(offset.x, offset.y, 0);
    }
}
