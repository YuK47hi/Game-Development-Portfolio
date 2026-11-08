using UnityEngine;
using TMPro;

public class TickerText : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 100f; // 流れるスピード
    [SerializeField] private float resetPositionX = 600f; // リセットする右端のX座標
    [SerializeField] private float endPositionX = -600f; // 消える左端のX座標

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 左方向へ移動
        rectTransform.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

        // 左端まで行き切ったら右端に戻す（ループ処理）
        if (rectTransform.anchoredPosition.x < endPositionX)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.x = resetPositionX;
            rectTransform.anchoredPosition = pos;
        }
    }
}
