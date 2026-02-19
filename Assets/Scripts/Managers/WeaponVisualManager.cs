using UnityEngine;

public class WeaponVisualManager : MonoBehaviour
{
    [Header("무기 스킨 메쉬 오브젝트")]
    [SerializeField] private GameObject _greatSwordModel;
    [SerializeField] private GameObject _spearModel;
    [SerializeField] private GameObject _swordModel;
    [SerializeField] private GameObject _dualBladeModel;

    [Header("손에 든 무기 스킨")]
    [SerializeField] private GameObject _greatSwordSkin;
    [SerializeField] private GameObject _LdualBladeSkin;
    [SerializeField] private GameObject _RdualBladeSkin;
    [SerializeField] private GameObject _swordSkin;
    [SerializeField] private GameObject _shieldSkin;

    [Header("칼집/등에 맨 스킨")]
    [SerializeField] private GameObject _greatSwordSheathed;
    [SerializeField] private GameObject _greatSwordSheathedEmpty;
    [SerializeField] private GameObject _daggerScabbard_L_Empty;
    [SerializeField] private GameObject _daggerScabbard_L;
    [SerializeField] private GameObject _daggerScabbard_R_Empty;
    [SerializeField] private GameObject _daggerScabbard_R;
    [SerializeField] private GameObject _swordScabbard;
    [SerializeField] private GameObject _swordScabbardEmpty;

    private WeaponType _currentWeaponType;

    public void SetupVisuals(WeaponType type)
    {
        _currentWeaponType = type;

        DisableAllSkins(); 
        StowWeapon();

        switch (type)
        {
            case WeaponType.GreatSword:
                if (_greatSwordModel) _greatSwordModel.SetActive(true);
                break;

            case WeaponType.DualBlade:
                if (_dualBladeModel) _dualBladeModel.SetActive(true);
                break;

            case WeaponType.Spear:
                if (_spearModel) _spearModel.SetActive(true);
                break;

            case WeaponType.SwordShield:
                if (_swordModel) _swordModel.SetActive(true);
                if (_shieldSkin) _shieldSkin.SetActive(true);
                break;
        }
    }

    private void DisableAllSkins()
    {
        if (_greatSwordModel) _greatSwordModel.SetActive(false);
        if (_spearModel) _spearModel.SetActive(false);
        if (_swordModel) _swordModel.SetActive(false);
        if (_dualBladeModel) _dualBladeModel.SetActive(false);
  
        if (_greatSwordSkin) _greatSwordSkin.SetActive(false);
        if (_LdualBladeSkin) _LdualBladeSkin.SetActive(false);
        if (_RdualBladeSkin) _RdualBladeSkin.SetActive(false);
        if (_swordSkin) _swordSkin.SetActive(false);
        if (_shieldSkin) _shieldSkin.SetActive(false);
        if (_greatSwordSheathed) _greatSwordSheathed.SetActive(false);
        if (_greatSwordSheathedEmpty) _greatSwordSheathedEmpty.SetActive(false);
        if (_daggerScabbard_L_Empty) _daggerScabbard_L_Empty.SetActive(false);
        if (_daggerScabbard_L_Empty) _daggerScabbard_L.SetActive(false);
        if (_daggerScabbard_R_Empty) _daggerScabbard_R_Empty.SetActive(false);
        if (_daggerScabbard_R_Empty) _daggerScabbard_R.SetActive(false);
        if (_swordScabbard) _swordScabbard.SetActive(false);
        if (_swordScabbardEmpty) _swordScabbardEmpty.SetActive(false);
    }

    // [애니메이션 이벤트] 칼을 뽑는 순간 호출
    public void DrawWeapon()
    {
        switch (_currentWeaponType)
        {
            case WeaponType.GreatSword:
                if (_greatSwordSheathed) _greatSwordSheathed.SetActive(false);
                if (_greatSwordSheathedEmpty) _greatSwordSheathedEmpty.SetActive(true);
                if (_greatSwordSkin) _greatSwordSkin.SetActive(true);
                break;
            case WeaponType.DualBlade:
                if (_daggerScabbard_L_Empty) _daggerScabbard_L_Empty.SetActive(true); 
                if (_daggerScabbard_L_Empty) _daggerScabbard_L.SetActive(false); 
                if (_daggerScabbard_R_Empty) _daggerScabbard_R_Empty.SetActive(true); 
                if (_daggerScabbard_R_Empty) _daggerScabbard_R.SetActive(false); 
                if (_LdualBladeSkin) _LdualBladeSkin.SetActive(true);        
                if (_RdualBladeSkin) _RdualBladeSkin.SetActive(true);         
                break;
            case WeaponType.SwordShield:
                if (_swordScabbard) _swordScabbard.SetActive(false);
                if (_swordScabbardEmpty) _swordScabbardEmpty.SetActive(true);
                if (_swordSkin) _swordSkin.SetActive(true);
                break;
        }
    }

    // [애니메이션 이벤트] 칼을 넣는 순간 호출
    public void StowWeapon()
    {
        switch (_currentWeaponType)
        {
            case WeaponType.GreatSword:
                if (_greatSwordSkin) _greatSwordSkin.SetActive(false);         // 손에 있는거 끄기
                if (_greatSwordSheathed) _greatSwordSheathed.SetActive(true);  // 등에 있는거 켜기
                if (_greatSwordSheathedEmpty) _greatSwordSheathedEmpty.SetActive(false);  // 등에 있는거 켜기
                break;
            case WeaponType.DualBlade:
                if (_daggerScabbard_L_Empty) _daggerScabbard_L_Empty.SetActive(false);
                if (_daggerScabbard_L_Empty) _daggerScabbard_L.SetActive(true);
                if (_daggerScabbard_R_Empty) _daggerScabbard_R_Empty.SetActive(false);
                if (_daggerScabbard_R_Empty) _daggerScabbard_R.SetActive(true);
                if (_LdualBladeSkin) _LdualBladeSkin.SetActive(false);
                if (_RdualBladeSkin) _RdualBladeSkin.SetActive(false);
                break;
            case WeaponType.SwordShield:
                if (_swordSkin) _swordSkin.SetActive(false);
                if (_swordScabbard) _swordScabbard.SetActive(true);
                if (_swordScabbardEmpty) _swordScabbardEmpty.SetActive(false);
                break;
        }
    }
}