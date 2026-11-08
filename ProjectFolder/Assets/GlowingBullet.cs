using UnityEngine;

public class GlowingBullet : MonoBehaviour
{
    [Header("発光（Emission）の設定")]
    [ColorUsage(true, true)] // HDRカラーで強烈に発光させます
    public Color glowColor = Color.cyan * 4f; // 弾の発光色（水色ネオン風）

    [Header("周囲を照らす光（Point Light）")]
    public bool addLight = true;         // 周囲を照らすライトをつけるか
    public float lightRange = 8f;        // 照らす範囲
    public float lightIntensity = 3f;    // 照らす強さ

    private Material bulletMaterial;
    private Light bulletLight;

    void Awake()
    {
        // 1. マテリアルのEmission（自発光）を有効化して色を適用
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            bulletMaterial = renderer.material;

            // Emissionキーワードを有効化
            bulletMaterial.EnableKeyword("_EMISSION");
            // 強烈な発光色をセット
            bulletMaterial.SetColor("_EmissionColor", glowColor);
        }

        // 2. 弾の周辺をパッと照らすPoint Lightを自動生成・設定
        if (addLight)
        {
            bulletLight = GetComponent<Light>();
            if (bulletLight == null)
            {
                bulletLight = gameObject.AddComponent<Light>();
            }

            bulletLight.type = LightType.Point;
            // 発光色と同系統の色で周囲を照らす
            bulletLight.color = new Color(glowColor.r, glowColor.g, glowColor.b, 1f);
            bulletLight.range = lightRange;
            bulletLight.intensity = lightIntensity;
        }
    }
}
