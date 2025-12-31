using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnIndicator : MonoBehaviour
{

    //생성효과 초
    public float duration = 1.5f;


    //변수들
    Vector3 baseScale;
    MeshRenderer renderer1;
    Material mat;

    float nextGlitchTime = 0f;
    void Awake()
    {
        baseScale = transform.localScale;
        renderer1 = GetComponentInChildren<MeshRenderer>();
        mat = renderer1.material;
    }

    #region 글리치 효과
    public IEnumerator Play()
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            if (Time.time >= nextGlitchTime)
            {
                nextGlitchTime = Time.time + Random.Range(0.05f, 0.15f);

                float gx = 1f + Random.Range(-0.02f, 0.02f);
                float gy = 1f + Random.Range(-0.02f, 0.02f);
                transform.localScale = new Vector3(
                    baseScale.x * gx,
                    baseScale.y * gy,
                    baseScale.z
                );

                mat.mainTextureOffset = new Vector2(
                    Random.Range(-0.005f, 0.005f),
                    Random.Range(-0.005f, 0.005f)
                );

                if (Random.value < 0.1f) 
                    mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.2f);
                else
                    mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, Random.Range(0.6f, 0.9f));
            }

            yield return null;
        }

        transform.localScale = baseScale;
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.9f);
        mat.mainTextureOffset = Vector2.zero;
    }
    #endregion
}