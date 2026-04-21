using UnityEngine;
using UnityEngine.UI;

public class WeakPointUI : MonoBehaviour
{
    public Image fillImage;

    float maxHP;

    public void Init(int hp)
    {
        maxHP = hp;
        SetHP(hp);
    }

    public void SetHP(int currentHP)
    {
        fillImage.fillAmount = (float)currentHP / maxHP;
    }

    void LateUpdate()
    {
        if (Camera.main == null) return;

        transform.forward = Camera.main.transform.forward;
    }
}
