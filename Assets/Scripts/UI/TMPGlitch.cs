using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMPGlitch : MonoBehaviour      //이것들은 그냥 TMP 글리치효과 하기 위한 스크립트
{
    TextMeshProUGUI tmp;
    Mesh mesh;
    Vector3[] vertices;

    [Header("Vertex Glitch")]
    public float intensity = 2f;
    public float speed = 20f;

    [Header("Electronic Noise")]
    public float flickerChance = 0.1f;    
    public float shakeAmount = 2f;        
    public float minAlpha = 0.25f;       

    Vector3 basePos;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        basePos = tmp.rectTransform.anchoredPosition;
    }

    void Update()
    {
        ApplyVertexGlitch();
        ApplyElectronicNoise();
    }

    void ApplyVertexGlitch()
    {
        tmp.ForceMeshUpdate();

        mesh = tmp.mesh;
        vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            float noise = Mathf.PerlinNoise(Time.time * speed, i);
            vertices[i] += new Vector3((noise - 0.5f) * intensity, 0, 0);
        }

        mesh.vertices = vertices;
        tmp.canvasRenderer.SetMesh(mesh);
    }

    void ApplyElectronicNoise()
    {
        tmp.rectTransform.anchoredPosition =
            basePos + new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                0
            );
        if (Random.value < flickerChance)
        {
            float a = Random.Range(minAlpha, 1f);
            tmp.color = new Color(1, 1, 1, a);
        }
        else
        {
            tmp.color = new Color(1, 1, 1, 1f);
        }
    }
}
