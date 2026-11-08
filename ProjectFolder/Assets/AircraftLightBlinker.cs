using Unity.VisualScripting;
using UnityEngine;

public class AircraftLightBlinker : MonoBehaviour
{
    [Header("対象ライト")]
    public Light targetLight;

    [Header("点滅周期（秒）")]
    public float flashInterval = 0.8f;  // 点滅の間隔
    public float flashDuration = 0.1f;  // 発光している時間（パッと光らせる場合）

    [Header("点滅モード設定")]
    public bool isStrobeMode = true;    // true: 航空機風のパッと光るストロボ / false: 等間隔の点滅

    private float timer = 0f;

    void Start()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }
    }

    void Update()
    {
        if (targetLight == null) return;

        timer += Time.deltaTime;

        if (isStrobeMode)
        {
            // 航空機のストロボ風（一瞬だけパッと光る）
            if (timer >= flashInterval)
            {
                targetLight.enabled = true;
                if (timer >= flashInterval + flashDuration)
                {
                    targetLight.enabled = false;
                    timer = 0f;
                }
            }
            else
            {
                targetLight.enabled = false;
            }
        }
        else
        {
            // 定期点滅（ON/OFF切替）
            if (timer >= flashInterval)
            {
                targetLight.enabled = !targetLight.enabled;
                timer = 0f;
            }
        }
    }
}
