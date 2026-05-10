using UnityEngine;

public class JumpWarningFill : MonoBehaviour
{
    private float duration = 1f;
    private float targetSize = 1f;
    private float timer = 0f;

    public void Init(float size, float showDuration)
    {
        targetSize = size;
        duration = showDuration;
        timer = 0f;

        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duration);

        transform.localScale = new Vector3(
            targetSize * t,
            targetSize * t,
            1f
        );
    }
}
