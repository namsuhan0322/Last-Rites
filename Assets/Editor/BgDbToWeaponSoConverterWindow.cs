using BansheeGz.BGDatabase;
using DB_;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BgDbToWeaponSoConverterWindow : EditorWindow
{
    #region 설정 필드
    private string m_outputFolder = "Assets/WeaponSOs";
    private WeaponDatabaseSO m_targetDatabaseSo;

    private bool m_overwriteExisting = true;
    private bool m_addToDatabase = true;
    private bool m_verboseLog = true;
    #endregion

    [MenuItem("Tools/BG DB → Create WeaponSO")]
    private static void OpenWindow()
    {
        var window = GetWindow<BgDbToWeaponSoConverterWindow>(true, "BG DB -> WeaponSO Converter");
        window.minSize = new Vector2(520, 250);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("BG Database → WeaponSO 변환기", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. 폴더 선택
        EditorGUILayout.LabelField("출력 폴더 (Assets/...)", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        m_outputFolder = EditorGUILayout.TextField(m_outputFolder);
        if (GUILayout.Button("폴더 선택", GUILayout.Width(100)))
        {
            string select = EditorUtility.OpenFolderPanel("Select output folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(select))
            {
                if (select.StartsWith(Application.dataPath))
                    m_outputFolder = "Assets" + select.Substring(Application.dataPath.Length);
                else
                    EditorUtility.DisplayDialog("오류", "선택한 폴더는 프로젝트의 Assets 폴더 내부에 있어야 합니다.", "확인");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 2. 타겟 DB SO 선택
        m_targetDatabaseSo = (WeaponDatabaseSO)EditorGUILayout.ObjectField("Target WeaponDatabaseSO", m_targetDatabaseSo, typeof(WeaponDatabaseSO), false);

        // 3. 옵션
        m_addToDatabase = EditorGUILayout.ToggleLeft("생성된 SO를 Database 리스트에 자동 등록", m_addToDatabase);
        m_overwriteExisting = EditorGUILayout.ToggleLeft("기존 SO 덮어쓰기 (ID/이름 일치 시)", m_overwriteExisting);
        m_verboseLog = EditorGUILayout.ToggleLeft("상세 로그 출력", m_verboseLog);

        EditorGUILayout.Space();

        // 4. 실행 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("변환 실행", GUILayout.Height(38)))
        {
            if (!Directory.Exists(m_outputFolder))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(m_outputFolder), Path.GetFileName(m_outputFolder));
                AssetDatabase.Refresh();
            }

            ConvertAllBgWeaponToSo();
        }

        if (GUILayout.Button("새 DatabaseSO 생성", GUILayout.Height(38), GUILayout.Width(180)))
        {
            CreateNewDatabaseSo();
        }
        EditorGUILayout.EndHorizontal();
    }

    #region 변환 로직

    private void ConvertAllBgWeaponToSo()
    {
        try
        {
            // DB_._Weapon 엔티티 가져오기
            var entities = GatherAllWeaponEntities();

            if (entities.Count == 0)
            {
                EditorUtility.DisplayDialog("알림", "BG Database에서 Weapon 데이터를 찾지 못했습니다.\n데이터가 비어있는지 확인해주세요.", "확인");
                return;
            }

            if (m_addToDatabase && m_targetDatabaseSo == null)
            {
                if (EditorUtility.DisplayDialog("DatabaseSO 미지정", "Target DatabaseSO가 없습니다. 새로 생성하시겠습니까?", "네", "아니요"))
                    CreateNewDatabaseSo();
                else
                    m_addToDatabase = false;
            }

            int total = entities.Count;
            int processed = 0;
            List<WeaponSO> createdSoList = new List<WeaponSO>();

            for (int i = 0; i < total; i++)
            {
                EditorUtility.DisplayProgressBar("WeaponSO 변환 중...", $"처리 중: {i + 1}/{total}", (float)i / total);
                var entity = entities[i];

                WeaponSO tempSo = ScriptableObject.CreateInstance<WeaponSO>();
                PopulateWeaponSOFromEntity(tempSo, entity);

                // 폴더 정리 (WeaponType별 분류)
                string typeFolder = Path.Combine(m_outputFolder, tempSo.weaponType.ToString()).Replace("\\", "/");
                if (!Directory.Exists(typeFolder)) AssetDatabase.CreateFolder(m_outputFolder, tempSo.weaponType.ToString());

                // 파일명 (ID_이름.asset)
                string fileName = SanitizeFileName($"{tempSo.WeaponID}_{tempSo.name}.asset");
                string assetPath = Path.Combine(typeFolder, fileName).Replace("\\", "/");

                WeaponSO finalSo = null;
                WeaponSO existing = AssetDatabase.LoadAssetAtPath<WeaponSO>(assetPath);

                if (existing != null && m_overwriteExisting)
                {
                    finalSo = existing;
                    PopulateWeaponSOFromEntity(finalSo, entity); // 덮어쓰기
                }
                else if (existing != null && !m_overwriteExisting)
                {
                    finalSo = existing;
                    if (m_verboseLog) Debug.Log($"스킵됨(이미 존재): {assetPath}");
                }
                else
                {
                    finalSo = tempSo;
                    AssetDatabase.CreateAsset(finalSo, AssetDatabase.GenerateUniqueAssetPath(assetPath));
                }

                EditorUtility.SetDirty(finalSo);
                createdSoList.Add(finalSo);
                processed++;
            }

            if (m_addToDatabase && m_targetDatabaseSo != null)
            {
                AddSOsToDatabase(createdSoList);
                EditorUtility.SetDirty(m_targetDatabaseSo);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("완료", $"변환 완료: 총 {processed}개 처리됨.", "확인");
        }
        catch (Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"변환 중 오류 발생: {ex}");
        }
    }

    /// <summary>
    /// 실제 데이터 매핑 (DB_._Weapon -> WeaponSO)
    /// </summary>
    private void PopulateWeaponSOFromEntity(WeaponSO so, DB_._Weapon entity)
    {
        if (so == null || entity == null) return;

        // 1. 기본 정보
        so.WeaponID = entity.WeaponID;
        so.name = entity.name;

        // 2. Enum 파싱 (DB의 Type 필드는 String임)
        // entity.Type -> WeaponType Enum
        string typeStr = entity.Type;
        if (Enum.TryParse(typeStr, true, out WeaponType parsedType))
        {
            so.weaponType = parsedType;
        }
        else
        {
            Debug.LogWarning($"WeaponType 파싱 실패 ({typeStr}). 기본값(GreatSword) 사용. ID: {entity.WeaponID}");
            so.weaponType = WeaponType.GreatSword;
        }

        // 3. 전투 스탯 매핑
        so.Atk_Spd = entity.Atk_Spd;

        so.Combo_1 = entity.Combo_1;
        so.Combo_2 = entity.Combo_2;
        so.Combo_3 = entity.Combo_3;

        so.Q_Dmg = entity.Q_Dmg;
        so.Q_Cool = entity.Q_Cool;

        so.W_Dmg = entity.W_Dmg;
        so.W_Cool = entity.W_Cool;

        so.E_Dmg = entity.E_Dmg;
        so.E_Cool = entity.E_Cool;

        so.R_Val = entity.R_Val;
        so.R_Cool = entity.R_Cool;

        so.V_Dmg = entity.V_Dmg;
        so.V_Cool = entity.V_Cool;
    }

    /// <summary>
    /// BGDatabase의 _Weapon 엔티티 수집
    /// </summary>
    private List<DB_._Weapon> GatherAllWeaponEntities()
    {
        var result = new List<DB_._Weapon>();
        try
        {
            // Generated Code의 CountEntities와 GetEntity 사용
            int count = DB_._Weapon.CountEntities;
            for (int i = 0; i < count; i++)
            {
                result.Add(DB_._Weapon.GetEntity(i));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"DB 조회 실패: {ex.Message}");
        }
        return result;
    }

    #endregion

    #region 유틸리티

    private void AddSOsToDatabase(List<WeaponSO> newSOs)
    {
        if (m_targetDatabaseSo == null) return;

        Undo.RecordObject(m_targetDatabaseSo, "Update Weapon List");
        m_targetDatabaseSo.Initialize();

        foreach (var newSo in newSOs)
        {
            if (newSo == null) continue;

            var existing = m_targetDatabaseSo.GetItemById(newSo.WeaponID);

            if (existing != null)
            {
                int index = m_targetDatabaseSo.weapons.IndexOf(existing);
                if (index >= 0) m_targetDatabaseSo.weapons[index] = newSo;
            }
            else
            {
                m_targetDatabaseSo.weapons.Add(newSo);
            }
        }
    }

    private void CreateNewDatabaseSo()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create WeaponDatabaseSO", "WeaponDatabase", "asset", "Save Location", "Assets");
        if (string.IsNullOrEmpty(path)) return;

        var dbSo = ScriptableObject.CreateInstance<WeaponDatabaseSO>();
        AssetDatabase.CreateAsset(dbSo, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        m_targetDatabaseSo = dbSo;
    }

    private string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    #endregion
}