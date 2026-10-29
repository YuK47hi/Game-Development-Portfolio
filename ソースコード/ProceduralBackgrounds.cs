using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NovelGame2D;

/// <summary>
/// コードによる計算（プロシージャル手法）で背景テクスチャを動的生成するクラス
/// </summary>
public static class ProceduralBackgrounds
{
    /// <summary>
    /// 深層コア（Deep Core）の背景テクスチャを生成する
    /// </summary>
    public static Texture2D CreateDeepCoreBg(GraphicsDevice device, int width = 1280, int height = 720)
    {
        Color[] pixels = new Color[width * height];
        Random rnd = new Random(123);

        Color topColor = new Color(3, 8, 20);
        Color bottomColor = new Color(10, 35, 60);

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / height;
            Color baseCol = Color.Lerp(topColor, bottomColor, t);

            for (int x = 0; x < width; x++)
            {
                Color c = baseCol;

                // 微細なサイバーグリッドを描画
                if (x % 80 == 0 || y % 80 == 0)
                {
                    c = Color.Lerp(c, Color.Cyan, 0.15f);
                }

                // ランダムなデジタルの星屑・ノードを描画
                if (rnd.Next(2500) == 0)
                {
                    c = Color.Lerp(c, Color.LightCyan, 0.6f);
                }

                pixels[y * width + x] = c;
            }
        }

        Texture2D tex = new Texture2D(device, width, height);
        tex.SetData(pixels);
        return tex;
    }
    /// <summary>
    /// 地平線（Horizon）の背景テクスチャを生成する
    /// </summary>
    public static Texture2D CreateHorizonBg(GraphicsDevice device, int width = 1280, int height = 720)
    {
        Color[] pixels = new Color[width * height];
        Random rnd = new Random(456);

        Color skyTop = new Color(10, 25, 65);
        Color horizonGold = new Color(255, 210, 120);
        Color groundDark = new Color(20, 15, 35);

        int horizonY = (int)(height * 0.58f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c;
                if (y < horizonY)
                {
                    float t = (float)y / horizonY;
                    // 指数関数的な光の広がりを表現
                    float glow = (float)Math.Pow(t, 3.5);
                    c = Color.Lerp(skyTop, horizonGold, glow);
                }
                else
                {
                    float t = (float)(y - horizonY) / (height - horizonY);
                    c = Color.Lerp(horizonGold * 0.6f, groundDark, t);
                    // 水面・地平線の反射グリッドを描画
                    if (y % 25 == 0 || Math.Abs(x - width / 2) % (int)(50 + t * 80) == 0)
                    {
                        c = Color.Lerp(c, Color.Gold, 0.25f);
                    }
                }

                pixels[y * width + x] = c;
            }
        }

        Texture2D tex = new Texture2D(device, width, height);
        tex.SetData(pixels);
        return tex;
    }
    /// <summary>
    /// ハーモニー（Harmony）の背景テクスチャを生成する
    /// </summary>
    public static Texture2D CreateHarmonyBg(GraphicsDevice device, int width = 1280, int height = 720)
    {
        Color[] pixels = new Color[width * height];
        Random rnd = new Random(789);

        Color topColor = new Color(15, 10, 38);
        Color bottomColor = new Color(40, 20, 55);

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / height;
            Color baseCol = Color.Lerp(topColor, bottomColor, t);

            for (int x = 0; x < width; x++)
            {
                Color c = baseCol;

                // 穏やかな星屑を描画
                if (rnd.Next(1200) == 0)
                {
                    float starBrightness = (float)rnd.NextDouble() * 0.7f + 0.3f;
                    c = Color.Lerp(c, Color.LightPink, starBrightness);
                }

                pixels[y * width + x] = c;
            }
        }

        Texture2D tex = new Texture2D(device, width, height);
        tex.SetData(pixels);
        return tex;
    }
    /// <summary>
    /// オデッセイ（Odyssey）の背景テクスチャを生成する
    /// </summary>
    public static Texture2D CreateOdysseyBg(GraphicsDevice device, int width = 1280, int height = 720)
    {
        Color[] pixels = new Color[width * height];
        Random rnd = new Random(999);

        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        float maxDist = (float)Math.Sqrt(center.X * center.X + center.Y * center.Y);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = x - center.X;
                float dy = y - center.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                float angle = (float)Math.Atan2(dy, dx);

                // ワームホール・ラジアルストリーム効果
                float stream = (float)Math.Sin(angle * 12f + dist * 0.05f);
                float normDist = dist / maxDist;

                Color c = Color.Lerp(new Color(5, 5, 20), new Color(15, 5, 35), normDist);
                if (stream > 0.6f)
                {
                    Color streamCol = (angle > 0) ? Color.DeepSkyBlue : Color.Magenta;
                    c = Color.Lerp(c, streamCol, (stream - 0.6f) * 1.5f * (1.0f - normDist * 0.5f));
                }

                if (dist < 80f)
                {
                    c = Color.Lerp(c, Color.White, 1.0f - (dist / 80f));
                }

                pixels[y * width + x] = c;
            }
        }

        Texture2D tex = new Texture2D(device, width, height);
        tex.SetData(pixels);
        return tex;
    }
}
