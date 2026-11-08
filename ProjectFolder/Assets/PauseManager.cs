using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseManager : MonoBehaviour
{
    [Header("UI参照")]
    public GameObject pausePanel;          // ポーズ画面
    public GameObject resultPanel;         // リザルト画面
    public TextMeshProUGUI scoreText;      // プレイ中のスコア表示
    public TextMeshProUGUI resultScoreText;// リザルト画面の最終スコア表示

    [Header("サウンド参照")]
    public AudioSource bgmSource;          // 追加: BGM用のAudioSource

    [Header("ゲーム設定")]
    public int targetScore = 1000;         // クリア目標スコア

    private bool isPaused = false;
    private bool isGameEnded = false;
    private int score = 0;

    void Start()
    {
        Time.timeScale = 1f;

        // 安全に非表示化
        if (pausePanel != null) pausePanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);

        UpdateScoreUI();
    }

    void Update()
    {
        if (isGameEnded) return;

        // Pキー または ESCキー
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    // スコア加算
    public void AddScore(int amount)
    {
        if (isGameEnded) return;

        score += amount;
        UpdateScoreUI();

        if (score >= targetScore)
        {
            CheckGameClear();
        }
    }

    // クリア判定
    void CheckGameClear()
    {
        isGameEnded = true;
        Time.timeScale = 0f;

        // ゲームクリア時にBGMを停止
        if (bgmSource != null)
        {
            bgmSource.Pause();
        }

        if (resultScoreText != null)
        {
            resultScoreText.text = "CLEAR SCORE: " + score;
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
    }

    // ポーズの切替
    public void TogglePause()
    {
        if (isGameEnded) return;

        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;

        // ポーズ状態に合わせてBGMを一時停止 / 再開
        if (bgmSource != null)
        {
            if (isPaused)
            {
                bgmSource.Pause();
            }
            else
            {
                bgmSource.UnPause();
            }
        }
    }

    // ゲーム再開（Resumeボタン用）
    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        // Resumeボタンで再開した際にもBGMを再開
        if (bgmSource != null)
        {
            bgmSource.UnPause();
        }
    }

    // リスタート（Restartボタン用）
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // タイトルへ戻る（Titleボタン用）
    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
}