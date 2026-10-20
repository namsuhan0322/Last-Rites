using System;

/// <summary>
/// [역할] 빌드 용량 판정 규칙(엔진 없음): 화면에 보이는 크기 → 텍스처 최대 크기, 최대 크기로 줄인 해상도,
///        블록 압축(DXT/BC) 가능 여부(4의 배수), 예상 바이트.
/// [이유] 압축 못 하는 큰 아이콘을 화면 크기에 맞춰 줄이는 규칙을 에디터 도구에서 떼어 엔진 밖에서 테스트하려고.
///        Unity는 줄인 크기가 4의 배수가 아니면 블록 압축을 못 하고 픽셀당 4바이트(RGBA32)로 넣는다(빌드 리포트로 확인).
/// [근거] 빌드 용량 명세 2.2·3.2, 결정 S6, S11
/// </summary>
public static class TextureBudgetRule
{
    public const int MinIconSize = 512;    // 1920×1080에서 아이콘이 보이는 크기(대부분 100~450px)보다 넉넉하게
    public const int MaxIconSize = 2048;   // Unity 기본 최대 크기. 화면 너비(1920)보다 크게 보이는 칸은 없음

    /// 보이는 크기 이상인 가장 작은 2의 거듭제곱 (MinIconSize ~ MaxIconSize). 0 = 못 잼 → 최소값
    public static int MaxSizeForDisplay(float displayPx)
    {
        int size = MinIconSize;
        while (size < displayPx && size < MaxIconSize) size *= 2;
        return size;
    }

    /// 긴 변을 maxSize에 맞춰 비율 그대로 줄인 해상도. 키우지 않음. 반올림이라 Unity 결과와 1px 다를 수 있음(명세 Z2)
    public static (int Width, int Height) ResizedSize(int width, int height, int maxSize)
    {
        int longest = Math.Max(width, height);
        if (longest <= maxSize) return (width, height);
        double scale = (double)maxSize / longest;
        int w = Math.Max(1, (int)Math.Round(width * scale, MidpointRounding.AwayFromZero));
        int h = Math.Max(1, (int)Math.Round(height * scale, MidpointRounding.AwayFromZero));
        return (w, h);
    }

    /// DXT/BC 블록 압축은 4×4 블록 단위라 가로·세로가 4의 배수여야 한다
    public static bool IsBlockCompressible(int width, int height) => width % 4 == 0 && height % 4 == 0;

    /// 픽셀당 바이트: 압축되면 1(BC6H·BC7·DXT5 기준, DXT1 0.5는 위로 잡음), 압축 못 하면 4(RGBA32), HDR은 8(RGBAHalf)
    public static float BytesPerPixel(bool compressed, int width, int height, bool hdr)
    {
        if (compressed && IsBlockCompressible(width, height)) return 1f;
        return hdr ? 8f : 4f;
    }

    /// 예상 바이트: 가로 × 세로 × 픽셀당 바이트, 밉맵이 있으면 × 4/3
    public static long EstimateBytes(int width, int height, float bytesPerPixel, bool mipmaps)
    {
        double bytes = (double)width * height * bytesPerPixel;
        if (mipmaps) bytes = bytes * 4.0 / 3.0;
        return (long)bytes;
    }
}
