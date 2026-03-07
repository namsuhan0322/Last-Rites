using UnityEngine;
using TMPro;

public class DirectionArrowUI : MonoBehaviour
{
    public Transform player;
    public Transform goal;

    public RectTransform arrowUI;

    public float screenOffset = 80f;

    void Update()
    {
        if (player == null || goal == null) return;

        Vector3 dir = goal.position - player.position;
        dir.y = 0;

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        arrowUI.rotation = Quaternion.Euler(0, 0, -angle + 90f);

        Vector3 center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        Vector2 offset = new Vector2(
            Mathf.Sin(angle * Mathf.Deg2Rad),
            Mathf.Cos(angle * Mathf.Deg2Rad)
        ) * screenOffset;

        arrowUI.position = center + new Vector3(offset.x, offset.y, 0);
    }
}
