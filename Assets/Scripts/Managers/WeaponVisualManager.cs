using UnityEngine;

public class WeaponVisualManager : MonoBehaviour
{
    [Header("무기 스킨 메쉬 오브젝트")]
    [SerializeField] private GameObject _greatSwordSkin;
    [SerializeField] private GameObject _spearSkin;
    [SerializeField] private GameObject _swordSkin;
    [SerializeField] private GameObject _dualBladeSkin;
    [SerializeField] private GameObject _shieldSkin;

    public void SetupVisuals(WeaponType type)
    {
        DisableAllSkins();

        switch (type)
        {
            case WeaponType.GreatSword:
                if (_greatSwordSkin) _greatSwordSkin.SetActive(true);
                break;

            case WeaponType.DualBlade:
                if (_dualBladeSkin) _dualBladeSkin.SetActive(true);
                break;

            case WeaponType.Spear:
                if (_spearSkin) _spearSkin.SetActive(true);
                break;

            case WeaponType.SwordShield:
                if (_swordSkin) _swordSkin.SetActive(true);
                if (_shieldSkin) _shieldSkin.SetActive(true);
                break;
        }
    }

    private void DisableAllSkins()
    {
        if (_greatSwordSkin) _greatSwordSkin.SetActive(false);
        if (_spearSkin) _spearSkin.SetActive(false);
        if (_swordSkin) _swordSkin.SetActive(false);
        if (_dualBladeSkin) _dualBladeSkin.SetActive(false);
        if (_shieldSkin) _shieldSkin.SetActive(false);
    }
}