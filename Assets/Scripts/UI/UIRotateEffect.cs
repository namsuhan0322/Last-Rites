using UnityEngine;

public class UIRotateEffect : MonoBehaviour
{
    public float rotateSpeed = 120f;

    private void Update()
    {
        transform.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);
    }
}
