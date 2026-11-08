using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlsUIManager : MonoBehaviour
{
    void Update()
    {
        // キーボードのどれかのキー（またはマウス）が押された瞬間
        if (Input.anyKeyDown)
        {
            StartGame();
        }
    }

    // ゲーム開始（ステージへ移動）
    public void StartGame()
    {
        // 一旦 "SampleScene" に直接遷移させる
        SceneManager.LoadScene("SampleScene");
    }
}