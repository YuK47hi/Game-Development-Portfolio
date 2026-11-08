using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // 選んだステージを記憶する変数（最初は宇宙シーンにしておく）
    // "SampleScene" の部分は、実際の宇宙シーンの名前に合わせてください
    public static string selectedStage = "SampleScene";

    // 「Stage Select」の中の【宇宙ボタン】を押した時
    public void SelectSpaceStage()
    {
        selectedStage = "SampleScene"; // 宇宙シーンの名前
        Debug.Log("宇宙ステージが選択されました！");
    }

    // 「Stage Select」の中の【日本ボタン】を押した時
    public void SelectJapanStage()
    {
        selectedStage = "Stage2_JapanCity"; // これから作る日本のシーンの名前
        Debug.Log("日本の街ステージが選択されました！");
    }

    // 「Start」ボタンを押した時
    public void StartGame()
    {
        // いきなりゲームに行かず、まずは操作説明シーンへ！
        SceneManager.LoadScene("ControlsScene");
    }

    // 「Quit」ボタンを押した時
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}