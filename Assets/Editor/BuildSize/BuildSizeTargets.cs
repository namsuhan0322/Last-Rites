/// <summary>
/// [역할] 빌드 용량 도구가 바꿀 대상의 명시적인 표(경로와 값). 규칙이 아니라 조사 결과다.
/// [이유] 무엇이 바뀌는지 미리 보고 검토할 수 있게 표로 둔다(결정 S3). 새 에셋은 자동으로 들어오지 않는다.
/// [근거] 빌드 용량 명세 2장·3.2. 출처: 2026-10-20 빌드 리포트(DevDocs/BuildSize/report_2026-10-20_before.txt),
///        아이콘이 보이는 크기는 DevDocs/BuildSize/tools/icon_table.py 정적 분석(근사, 명세 Z1)
/// </summary>
public static class BuildSizeTargets
{
    // ① 로비 라이트맵 (결정 S5): 이 폴더의 Lightmap-*_comp_light.exr 14장 + Lightmap-*_comp_dir.png 14장
    public const string LobbyLightmapFolder = "Assets/Scenes/LobbyScene";
    // 로비가 쓰는 라이팅 설정 (다운로드 팩 파일, 팩 데모 씬도 쓰지만 데모 씬은 빌드 밖)
    public const string LobbyLightingSettings = "Assets/DownloadAssets/Fantastic Dungeon Pack/scenes/Settings.lighting";

    // ② 압축 못 하는 아이콘·UI 이미지 1MB 이상 34개 (결정 S6): (경로, 1920×1080에서 보이는 가장 큰 크기 px, 0 = 못 잼)
    //    실행 중에 넣는 칸은 잴 수 있는 칸 중 가장 큰 838px(rightFloorImage). 주석은 최대 크기와 리포트 → 예상 MB
    public static readonly (string Path, float DisplayPx)[] Icons =
    {
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/crystal bag.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 15.4 → 3.84MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/Pouch of crystal.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 15.3 → 3.82MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/health_potion .png", 83f),   // 빌드 씬·프리팹 칸: 최대 512, 15.1 → 0.99MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/pouch.png", 0f),   // 못 잼 → 최소값: 최대 512, 14.8 → 0.96MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/bag_1.png", 0f),   // 못 잼 → 최소값: 최대 512, 12.7 → 0.92MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/flour .png", 0f),   // 못 잼 → 최소값: 최대 512, 12.4 → 0.92MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/barrel_of_crystal.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 10.9 → 0.68MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/components/InfoBox.png", 893f),   // 빌드 씬·프리팹 칸: 최대 1024, 9.4 → 2.54MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/bread .png", 0f),   // 못 잼 → 최소값: 최대 512, 9.2 → 0.62MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/scroll.png", 0f),   // 못 잼 → 최소값: 최대 512, 9.1 → 0.8MB
        ("Assets/UI/ItemImage/테마3.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.8 → 2.18MB
        ("Assets/UI/ItemImage/테마2.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.7 → 2.18MB
        ("Assets/UI/ItemImage/오염된 야수의 뼈.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.7 → 2.18MB
        ("Assets/UI/ItemImage/튜토리얼 보스 소울.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.7 → 2.18MB
        ("Assets/UI/ItemImage/테마1.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.7 → 2.18MB
        ("Assets/UI/ItemImage/심연의 쐐기석.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.7 → 2.18MB
        ("Assets/UI/ItemImage/심연의 결정.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.7 → 2.18MB
        ("Assets/UI/ItemImage/일그러진 재의 파편.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.7 → 2.18MB
        ("Assets/UI/ItemImage/망자의 유물.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.7 → 2.18MB
        ("Assets/UI/ItemImage/정화된 뼛가루.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.7 → 2.18MB
        ("Assets/UI/ItemImage/진혼의 쐐기석.png", 838f),   // 실행 중에 넣는 칸: 최대 1024, 8.7 → 2.18MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/silverl_bar .png", 0f),   // 못 잼 → 최소값: 최대 512, 7.8 → 0.63MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/metal_bar .png", 0f),   // 못 잼 → 최소값: 최대 512, 7.8 → 0.63MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/gold_bar.png", 0f),   // 못 잼 → 최소값: 최대 512, 7.8 → 0.63MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/eggs.png", 0f),   // 못 잼 → 최소값: 최대 512, 7.2 → 0.53MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/axe_1.png", 0f),   // 못 잼 → 최소값: 최대 512, 7.2 → 0.13MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/components/description_box_01.png", 733f),   // 빌드 씬·프리팹 칸: 최대 1024, 6.6 → 3.31MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/components/crafting_wheel.png", 0f),   // 못 잼 → 최소값: 최대 512, 6.0 → 0.99MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/components/golden_ring_transparent.png", 300f),   // 빌드 씬·프리팹 칸: 최대 512, 6.0 → 0.25MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/colored_icon/Original/key .png", 0f),   // 못 잼 → 최소값: 최대 512, 5.8 → 0.41MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/components/description_box_03.png", 2096f),   // 빌드 씬·프리팹 칸: 최대 2048, 5.7 → 5.72MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/components/Glow.png", 3371f),   // 빌드 씬·프리팹 칸: 최대 2048, 5.7 → 5.67MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/components/mission_slot_02.png", 143f),   // 빌드 씬·프리팹 칸: 최대 512, 1.8 → 0.11MB
        ("Assets/DownloadAssets/GUI/ResourcesData/Sprites/components/paper.png", 0f),   // 못 잼 → 최소값: 최대 512, 1.2 → 0.99MB
    };

    // ③ 압축 꺼짐·원본 4096 텍스처 (결정 S7): PC 최대 LargeTextureMaxSize, 압축 켬
    public const int LargeTextureMaxSize = 2048;
    public static readonly string[] LargeTextures =
    {
        "Assets/DownloadAssets/BK/PureNature_Glacier/Textures/Surfaces/Icelake_n.png",   // 4096 압축 꺼짐 85.3MB
        "Assets/DownloadAssets/BK/PureNature_Glacier/Textures/Surfaces/Icelake_a.png",   // 4096 21.3MB
        "Assets/DownloadAssets/Malbers Animations/Dragons/8 - Kuzar the Magnificent/Materials_Textures/T_Kuzar_Wings_Normal.png",
        "Assets/DownloadAssets/Malbers Animations/Dragons/8 - Kuzar the Magnificent/Materials_Textures/T_Kuzar_Body_Normal.png",
        "Assets/DownloadAssets/Malbers Animations/Dragons/8 - Kuzar the Magnificent/Materials_Textures/T_Kuzar_Body_EmissionBW.png",
        "Assets/DownloadAssets/Malbers Animations/Dragons/8 - Kuzar the Magnificent/Materials_Textures/White/T_Kuzar_White_Body_Albedo.png",
        "Assets/DownloadAssets/Malbers Animations/Dragons/8 - Kuzar the Magnificent/Materials_Textures/White/T_Kuzar_White_Body_Rough_M_AO.png",
        "Assets/DownloadAssets/Malbers Animations/Dragons/8 - Kuzar the Magnificent/Materials_Textures/Green/T_Kuzar_Green_Body_Albedo.png",
        "Assets/DownloadAssets/Malbers Animations/Dragons/8 - Kuzar the Magnificent/Materials_Textures/Green/T_Kuzar_Green_Body_Rough_M_AO.png",
        "Assets/DownloadAssets/Malbers Animations/Dragons/8 - Kuzar the Magnificent/Materials_Textures/Green/T_Kuzar_Green_Wings_Albedo.png",
        "Assets/DownloadAssets/Malbers Animations/Dragons/8 - Kuzar the Magnificent/Materials_Textures/Green/T_Kuzar_Green_Wings_Rough_M_AO.png",
        "Assets/DownloadAssets/Enemy/Textures/AlphaCircle.png",                          // 1024 압축 꺼짐 4MB
    };

    // ④ 폰트 (결정 S8): 인스펙터에서 사용자가 아틀라스 FontAtlasSize·Multi Atlas Textures로 바꾸고, 도구는 확인만
    public const int FontAtlasSize = 2048;
    public static readonly string[] Fonts =
    {
        "Assets/Fonts/HeirofLightRegular SDF.asset",
        "Assets/Fonts/HeirofLightBold SDF.asset",
        "Assets/Fonts/HeirofLightBold SDF Shdow.asset",
    };
}
