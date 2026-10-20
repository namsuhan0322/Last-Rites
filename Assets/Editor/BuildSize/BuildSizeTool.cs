using System;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// [역할] 빌드 용량 도구. 메뉴 `SuHan/Build Size/미리 보기`는 바뀔 항목과 예상 크기를 Console에만 찍고,
///        `SuHan/Build Size/적용`은 표(BuildSizeTargets)대로 가져오기 설정을 바꾸고 다시 가져온다. 이미 맞으면 건너뛴다.
/// [이유] 70여 개 설정을 손으로 바꾸면 실수하기 쉽고, 무엇이 바뀌는지 먼저 봐야 안전하다.
///        텍스처는 PC(Standalone) 덮어쓰기만 바꿔 기본값과 원본을 지킨다(덮어쓰기를 끄면 원래대로).
/// [근거] 빌드 용량 명세 3장, 결정 S3~S8
/// </summary>
public static class BuildSizeTool
{
    private const string Standalone = "Standalone";

    // 항목 하나의 집계와 줄별 기록
    private class Tally
    {
        public readonly string Name;
        public int Change, Ok, Missing;
        public long Before, After;
        public readonly StringBuilder Lines = new StringBuilder();
        public Tally(string name) { Name = name; }
    }

    [MenuItem("SuHan/Build Size/미리 보기")]
    public static void Preview() => Run(false);

    [MenuItem("SuHan/Build Size/적용")]
    public static void Apply()
    {
        if (!EditorUtility.DisplayDialog("빌드 용량 적용",
                "표(BuildSizeTargets)대로 텍스처 가져오기 설정과 로비 라이팅 설정을 바꾸고 다시 가져옵니다.\n몇 분 걸릴 수 있습니다.",
                "적용", "취소")) return;
        Run(true);
    }

    private static void Run(bool apply)
    {
        var lightmaps = new Tally("① 로비 라이트맵");
        var icons = new Tally("② 아이콘·UI 이미지");
        var large = new Tally("③ 압축 꺼짐·4096 텍스처");
        var fonts = new Tally("④ 폰트 (인스펙터에서 직접)");

        AssetDatabase.StartAssetEditing();   // 다시 가져오기를 모아 한 번에
        try
        {
            DoLightmaps(lightmaps, apply);
            DoIcons(icons, apply);
            DoLargeTextures(large, apply);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        DoFonts(fonts);

        Tally[] all = { lightmaps, icons, large, fonts };
        string verb = apply ? "바꾼 것" : "바뀔 것";
        var sb = new StringBuilder();
        sb.AppendLine($"[BuildSize][{(apply ? "적용" : "미리 보기")}] {verb} {all.Sum(t => t.Change)} · 이미 맞음 {all.Sum(t => t.Ok)} · 파일 없음 {all.Sum(t => t.Missing)} · 예상 {MB(all.Sum(t => t.Before))} → {MB(all.Sum(t => t.After))}");
        foreach (Tally t in all)
        {
            sb.AppendLine($"── {t.Name}: {verb} {t.Change} · 이미 맞음 {t.Ok} · 파일 없음 {t.Missing} · 예상 {MB(t.Before)} → {MB(t.After)}");
            sb.Append(t.Lines);
        }
        Debug.Log(sb.ToString());
    }

    // ① 로비 라이트맵: 기본값 압축을 고품질로(라이팅 설정과 같게), 라이팅 설정도 High Quality (결정 S5)
    private static void DoLightmaps(Tally t, bool apply)
    {
        var settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(BuildSizeTargets.LobbyLightingSettings);
        if (settings == null)
        {
            t.Missing++;
            t.Lines.AppendLine($"   없음: {BuildSizeTargets.LobbyLightingSettings}");
        }
        else if (settings.lightmapCompression == LightmapCompression.HighQuality) t.Ok++;
        else
        {
            t.Change++;
            t.Lines.AppendLine($"   라이팅 설정 Lightmap Compression {settings.lightmapCompression} → HighQuality");
            if (apply)
            {
                settings.lightmapCompression = LightmapCompression.HighQuality;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssetIfDirty(settings);
            }
        }

        string[] files = Directory.GetFiles(BuildSizeTargets.LobbyLightmapFolder)
            .Select(f => f.Replace('\\', '/'))
            .Where(f => f.EndsWith("_comp_light.exr") || f.EndsWith("_comp_dir.png"))
            .OrderBy(f => f).ToArray();
        int changed = 0;
        foreach (string path in files)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) { t.Missing++; t.Lines.AppendLine($"   없음: {path}"); continue; }

            TextureImporterPlatformSettings eff = Effective(imp);
            bool hdr = path.EndsWith(".exr");
            imp.GetSourceTextureWidthAndHeight(out int w, out int h);
            var (bw, bh) = TextureBudgetRule.ResizedSize(w, h, eff.maxTextureSize);
            long before = Estimate(bw, bh, eff.textureCompression != TextureImporterCompression.Uncompressed, hdr, imp.mipmapEnabled);
            t.Before += before;
            if (eff.textureCompression == TextureImporterCompression.CompressedHQ) { t.Ok++; t.After += before; continue; }

            t.After += Estimate(bw, bh, true, hdr, imp.mipmapEnabled);
            t.Change++;
            changed++;
            if (apply)
            {
                imp.textureCompression = TextureImporterCompression.CompressedHQ;   // 기본값 (라이트맵은 라이팅 설정이 관리)
                TextureImporterPlatformSettings pc = imp.GetPlatformTextureSettings(Standalone);
                if (pc.overridden)
                {
                    pc.textureCompression = TextureImporterCompression.CompressedHQ;
                    imp.SetPlatformTextureSettings(pc);
                }
                imp.SaveAndReimport();
            }
        }
        t.Lines.AppendLine($"   라이트맵·방향맵 {files.Length}개 중 {changed}개 압축 없음 → 고품질 (최대 크기 그대로)");
    }

    // ② 압축 못 하는 아이콘: PC 덮어쓰기로 최대 크기만 보이는 크기에 맞춤, 압축 설정은 지금 그대로 (결정 S4·S6)
    private static void DoIcons(Tally t, bool apply)
    {
        foreach ((string path, float displayPx) in BuildSizeTargets.Icons)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) { t.Missing++; t.Lines.AppendLine($"   없음: {path}"); continue; }

            int target = TextureBudgetRule.MaxSizeForDisplay(displayPx);
            TextureImporterPlatformSettings eff = Effective(imp);
            bool compressed = eff.textureCompression != TextureImporterCompression.Uncompressed;
            imp.GetSourceTextureWidthAndHeight(out int w, out int h);
            var (bw, bh) = TextureBudgetRule.ResizedSize(w, h, eff.maxTextureSize);
            long before = Estimate(bw, bh, compressed, false, imp.mipmapEnabled);
            t.Before += before;
            if (eff.maxTextureSize <= target) { t.Ok++; t.After += before; continue; }

            var (aw, ah) = TextureBudgetRule.ResizedSize(w, h, target);
            long after = Estimate(aw, ah, compressed, false, imp.mipmapEnabled);
            t.After += after;
            t.Change++;
            t.Lines.AppendLine($"   {Path.GetFileName(path)}: 보이는 크기 {displayPx:F0}px, 최대 {eff.maxTextureSize} → {target} ({MB(before)} → {MB(after)})");
            if (apply)
            {
                SetStandalone(imp, target, eff.textureCompression);
                imp.SaveAndReimport();
            }
        }
    }

    // ③ 압축 꺼짐·원본 4096: PC 덮어쓰기로 최대 2048, 압축 꺼져 있으면 Compressed (켜져 있으면 그 품질 그대로) (결정 S4·S7)
    private static void DoLargeTextures(Tally t, bool apply)
    {
        int max = BuildSizeTargets.LargeTextureMaxSize;
        foreach (string path in BuildSizeTargets.LargeTextures)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) { t.Missing++; t.Lines.AppendLine($"   없음: {path}"); continue; }

            TextureImporterPlatformSettings eff = Effective(imp);
            bool compressed = eff.textureCompression != TextureImporterCompression.Uncompressed;
            imp.GetSourceTextureWidthAndHeight(out int w, out int h);
            var (bw, bh) = TextureBudgetRule.ResizedSize(w, h, eff.maxTextureSize);
            long before = Estimate(bw, bh, compressed, false, imp.mipmapEnabled);
            t.Before += before;
            if (eff.maxTextureSize <= max && compressed) { t.Ok++; t.After += before; continue; }

            int targetMax = Math.Min(eff.maxTextureSize, max);
            TextureImporterCompression targetCompression = compressed ? eff.textureCompression : TextureImporterCompression.Compressed;
            var (aw, ah) = TextureBudgetRule.ResizedSize(w, h, targetMax);
            long after = Estimate(aw, ah, true, false, imp.mipmapEnabled);
            t.After += after;
            t.Change++;
            t.Lines.AppendLine($"   {Path.GetFileName(path)}: 최대 {eff.maxTextureSize} → {targetMax}, 압축 {eff.textureCompression} → {targetCompression} ({MB(before)} → {MB(after)})");
            if (apply)
            {
                SetStandalone(imp, targetMax, targetCompression);
                imp.SaveAndReimport();
            }
        }
    }

    // ④ 폰트: TMP가 아틀라스 크기 바꾸기를 내부 함수로만 제공해 인스펙터에서 사용자가 바꾼다. 여기서는 확인만 (결정 S8)
    private static void DoFonts(Tally t)
    {
        int size = BuildSizeTargets.FontAtlasSize;
        foreach (string path in BuildSizeTargets.Fonts)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null) { t.Missing++; t.Lines.AppendLine($"   없음: {path}"); continue; }

            int pages = font.atlasTextures != null ? Math.Max(1, font.atlasTextures.Length) : 1;
            long before = (long)font.atlasWidth * font.atlasHeight * pages;   // Alpha8 = 1바이트/픽셀, 밉맵 없음
            t.Before += before;
            if (font.atlasWidth <= size && font.atlasHeight <= size && font.isMultiAtlasTexturesEnabled) { t.Ok++; t.After += before; continue; }

            t.After += (long)size * size;
            t.Change++;
            t.Lines.AppendLine($"   {Path.GetFileName(path)}: 아틀라스 {font.atlasWidth}×{font.atlasHeight}, 여러 장 {(font.isMultiAtlasTexturesEnabled ? "켬" : "꺼짐")} → 인스펙터에서 Atlas Width·Height {size}, Multi Atlas Textures 켬, Apply");
        }
    }

    // 지금 PC 빌드에 쓰이는 설정: PC 덮어쓰기가 켜져 있으면 그것, 아니면 기본값
    private static TextureImporterPlatformSettings Effective(TextureImporter imp)
    {
        TextureImporterPlatformSettings pc = imp.GetPlatformTextureSettings(Standalone);
        return pc.overridden ? pc : imp.GetDefaultPlatformTextureSettings();
    }

    // PC 덮어쓰기를 켜고 최대 크기·압축을 정한다. 형식은 자동, 압축 품질·크런치·축소 방식은 기본값을 따른다
    private static void SetStandalone(TextureImporter imp, int maxSize, TextureImporterCompression compression)
    {
        TextureImporterPlatformSettings def = imp.GetDefaultPlatformTextureSettings();
        TextureImporterPlatformSettings pc = imp.GetPlatformTextureSettings(Standalone);
        pc.overridden = true;
        pc.maxTextureSize = maxSize;
        pc.textureCompression = compression;
        pc.format = TextureImporterFormat.Automatic;
        pc.compressionQuality = def.compressionQuality;
        pc.crunchedCompression = def.crunchedCompression;
        pc.resizeAlgorithm = def.resizeAlgorithm;
        imp.SetPlatformTextureSettings(pc);
    }

    private static long Estimate(int w, int h, bool compressed, bool hdr, bool mipmaps)
        => TextureBudgetRule.EstimateBytes(w, h, TextureBudgetRule.BytesPerPixel(compressed, w, h, hdr), mipmaps);

    private static string MB(long bytes) => $"{bytes / 1048576f:F1}MB";
}
