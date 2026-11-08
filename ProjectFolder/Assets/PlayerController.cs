using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float xRange = 15f;
    public float yRange = 10f;

    // --- UIオブジェクトの設定 ---
    [Header("UI Objects")]
    public GameObject gameOverTextObj;
    public GameObject gameClearTextObj;
    public TextMeshProUGUI scoreText;

    // 追加：被弾時の画面フラッシュ用UI
    [Tooltip("画面全体を覆う真っ赤なImageをセットしてください")]
    public Image damageFlashImage;
    private Color flashColor = new Color(1f, 0f, 0f, 0.5f); // 赤色・半透明

    public GameObject explosionPrefab;
    public GameObject bullet;
    public Transform firePoint;

    // 追加：弾を撃った瞬間の光
    [Header("Effects")]
    public Light muzzleFlashLight;

    // --- HPゲージ用の設定 ---
    [Header("HP Settings")]
    public Slider hpSlider;
    public float maxHp = 100f;
    private float currentHp;
    private Image hpBarImage;

    // --- 戦闘機の回転回避設定 ---
    [Header("Rolling Settings")]
    public Transform vehicleModel;
    public float rollDuration = 0.35f;
    private bool isRolling = false;
    private Collider playerCollider;

    // --- リアルタイム背景＆太陽光変化の設定 ---
    [Header("Background & Sun Settings")]
    public Camera mainCamera;
    public Light sunLight;

    public bool useDebugHour = false;
    [Range(0f, 23.99f)]
    public float debugHour = 12f;

    [Header("Timer Settings")]
    public float timeRemaining = 180f;
    private bool isGameActive = true;

    private Material skyboxMaterial;

    void Start()
    {
        // UIの初期化
        if (gameOverTextObj != null) gameOverTextObj.SetActive(false);
        if (gameClearTextObj != null) gameClearTextObj.SetActive(false);
        // 被弾フラッシュ用パネルを透明に初期化
        if (damageFlashImage != null)
        {
            Color c = damageFlashImage.color;
            c.a = 0f; // 最初は透明にしておく
            damageFlashImage.color = c;
        }

        if (muzzleFlashLight != null) muzzleFlashLight.enabled = false;

        currentHp = maxHp;
        isGameActive = true;
        playerCollider = GetComponent<Collider>();

        // HPバーの初期化
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = maxHp;
            if (hpSlider.fillRect != null) hpBarImage = hpSlider.fillRect.GetComponent<Image>();
        }

        // スカイボックスのマテリアルを複製して個別適用
        if (RenderSettings.skybox != null)
        {
            skyboxMaterial = Instantiate(RenderSettings.skybox);
            RenderSettings.skybox = skyboxMaterial;
        }

        UpdateHPBarColor();
        UpdateAmbientColor();
    }

    void Update()
    {
        //  ダメージフラッシュを徐々に透明に戻す処理
        if (damageFlashImage != null && damageFlashImage.color.a > 0)
        {
            Color c = damageFlashImage.color;
            c.a -= Time.deltaTime * 2f; // 2fは消えるスピード
            damageFlashImage.color = c;
        }

        // ゲームオーバー/クリア時のリトライ＆タイトル移動入力処理
        if (!isGameActive && Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (Keyboard.current.fKey.wasPressedThisFrame) SceneManager.LoadScene("TitleScene");
            return;
        }

        if (!isGameActive) return;

        // ライティングとスカイボックスの回転更新
        UpdateAmbientColor();
        if (skyboxMaterial != null && skyboxMaterial.HasProperty("_Rotation"))
        {
            skyboxMaterial.SetFloat("_Rotation", Time.time * 1.5f);
        }

        // 弾の発射入力
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Shoot();
        }

        // バレルロール（回転回避）入力
        if (!isRolling && Keyboard.current != null)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame) StartCoroutine(PerformRoll(-360f));
            if (Keyboard.current.eKey.wasPressedThisFrame) StartCoroutine(PerformRoll(360f));
        }

        // 制限時間タイマー処理
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0) timeRemaining = 0;
            if (scoreText != null) scoreText.text = "Time: " + Mathf.CeilToInt(timeRemaining).ToString() + " s";
        }

        if (timeRemaining <= 0) TriggerGameClear();
        
        // 自機の移動処理
        float moveX = 0;
        float moveY = 0;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY = -1f;
        }

        transform.Translate(new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime);
        // 移動範囲の制限（クランプ）
        float clampedX = Mathf.Clamp(transform.position.x, -xRange, xRange);
        float clampedY = Mathf.Clamp(transform.position.y, -yRange, yRange);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
    // --- 弾の生成（SimplePool使用） ---
    void Shoot()
    {
        if (bullet != null && firePoint != null)
        {
            SimplePool.Spawn(bullet, firePoint.position, firePoint.rotation);
            // 弾を撃った瞬間に光らせる
            if (muzzleFlashLight != null) StartCoroutine(ShowMuzzleFlash());
        }
    }
    // マズルフラッシュ（発射光）の点灯処理
    IEnumerator ShowMuzzleFlash()
    {
        muzzleFlashLight.enabled = true;
        yield return new WaitForSeconds(0.05f); // 0.05秒だけピカッと光る
        muzzleFlashLight.enabled = false;
    }

    // 時間帯に応じた環境光・太陽光・スカイボックス色の更新
    void UpdateAmbientColor()
    {
        if (mainCamera == null) return;
        float currentHour;
        if (useDebugHour) currentHour = debugHour;
        else
        {
            System.DateTime now = System.DateTime.Now;
            currentHour = now.Hour + (now.Minute / 60f) + (now.Second / 3600f);
        }

        float morningExposure = 0.8f; float dayExposure = 0.85f; float eveningExposure = 0.5f; float nightExposure = 0.15f;
        Color morningTint = new Color(1f, 0.8f, 0.8f); Color dayTint = Color.white; Color eveningTint = new Color(1f, 0.5f, 0.3f); Color nightTint = new Color(0.2f, 0.2f, 0.5f);
        Color morningSun = new Color(1f, 0.6f, 0.4f); Color daySun = new Color(1f, 1f, 0.9f); Color eveningSun = new Color(1f, 0.3f, 0.1f); Color nightSun = new Color(0.1f, 0.15f, 0.3f);
        Vector3 morningRot = new Vector3(15f, -90f, 0f); Vector3 dayRot = new Vector3(90f, -90f, 0f); Vector3 eveningRot = new Vector3(165f, -90f, 0f); Vector3 nightRot = new Vector3(270f, -90f, 0f);
        float morningIntensity = 0.8f; float dayIntensity = 0.8f; float eveningIntensity = 0.7f; float nightIntensity = 0.1f;

        float finalExposure; Color finalTint; Color finalSunColor; Vector3 finalSunRot; float finalSunIntensity;

        if (currentHour >= 5f && currentHour < 11f) { float t = (currentHour - 5f) / 6f; finalExposure = Mathf.Lerp(morningExposure, dayExposure, t); finalTint = Color.Lerp(morningTint, dayTint, t); finalSunColor = Color.Lerp(morningSun, daySun, t); finalSunRot = Vector3.Lerp(morningRot, dayRot, t); finalSunIntensity = Mathf.Lerp(morningIntensity, dayIntensity, t); }
        else if (currentHour >= 11f && currentHour < 16f) { float t = (currentHour - 11f) / 5f; finalExposure = Mathf.Lerp(dayExposure, eveningExposure, t); finalTint = Color.Lerp(dayTint, eveningTint, t); finalSunColor = Color.Lerp(daySun, eveningSun, t); finalSunRot = Vector3.Lerp(dayRot, eveningRot, t); finalSunIntensity = Mathf.Lerp(dayIntensity, eveningIntensity, t); }
        else if (currentHour >= 16f && currentHour < 19f) { float t = (currentHour - 16f) / 3f; finalExposure = Mathf.Lerp(eveningExposure, nightExposure, t); finalTint = Color.Lerp(eveningTint, nightTint, t); finalSunColor = Color.Lerp(eveningSun, nightSun, t); finalSunRot = Vector3.Lerp(eveningRot, nightRot, t); finalSunIntensity = Mathf.Lerp(eveningIntensity, nightIntensity, t); }
        else { float t; if (currentHour >= 19f) t = (currentHour - 19f) / 10f; else t = (currentHour + 5f) / 10f; finalExposure = Mathf.Lerp(nightExposure, morningExposure, t); finalTint = Color.Lerp(nightTint, morningTint, t); finalSunColor = Color.Lerp(nightSun, morningSun, t); finalSunRot = Vector3.Lerp(nightRot, morningRot, t); finalSunIntensity = Mathf.Lerp(nightIntensity, morningIntensity, t); }

        if (skyboxMaterial != null) { if (skyboxMaterial.HasProperty("_Exposure")) skyboxMaterial.SetFloat("_Exposure", finalExposure); if (skyboxMaterial.HasProperty("_Tint")) skyboxMaterial.SetColor("_Tint", finalTint); }
        if (sunLight != null) { sunLight.color = finalSunColor; sunLight.transform.rotation = Quaternion.Euler(finalSunRot); sunLight.intensity = finalSunIntensity; }
    }

    // ロール回転アクション（無敵状態・コライダーOFF付き）
    IEnumerator PerformRoll(float targetAngle)
    {
        isRolling = true;
        float elapsed = 0f;
        if (playerCollider != null) playerCollider.enabled = false;

        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / rollDuration;
            float currentAngle = Mathf.Lerp(0f, targetAngle, percent);
            if (vehicleModel != null) vehicleModel.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
            yield return null;
        }

        if (vehicleModel != null) vehicleModel.localRotation = Quaternion.identity;
        if (playerCollider != null) playerCollider.enabled = true;
        isRolling = false;
    }
    // --- 障害物との衝突判定（SimplePool使用） ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (isRolling) return;
            TakeDamage(25f);
            SimplePool.Despawn(other.gameObject);
            // 変更: 爆発エフェクトを SimplePool から生成
            if (explosionPrefab != null)
            {
                SimplePool.Spawn(explosionPrefab, other.transform.position, Quaternion.identity);
            }
        }
    }
    // ダメージ処理＆HP判定
    void TakeDamage(float damageAmount)
    {
        if (!isGameActive) return;
        currentHp -= damageAmount;
        if (hpSlider != null) hpSlider.value = currentHp;

        // ダメージを受けたら画面を赤く光らせる！
        if (damageFlashImage != null) damageFlashImage.color = flashColor;

        UpdateHPBarColor();
        if (currentHp <= 0) TriggerGameOver();
    }
    // HPバーの残量カラー切り替え（緑→黄→赤）
    void UpdateHPBarColor()
    {
        if (hpBarImage == null || hpBarImage.Equals(null)) return;
        float hpRatio = currentHp / maxHp;
        if (hpRatio <= 0.334f) hpBarImage.color = Color.red;
        else if (hpRatio <= 0.667f) hpBarImage.color = Color.yellow;
        else hpBarImage.color = Color.green;
    }
    // ゲームクリア処理（画面上の全障害物を回収）
    void TriggerGameClear()
    {
        isGameActive = false;
        if (gameClearTextObj != null) gameClearTextObj.SetActive(true);
        if (gameOverTextObj != null) gameOverTextObj.SetActive(false);

        // 変更：残っている障害物を Destroy ではなく SimplePool.Despawn で回収
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            SimplePool.Despawn(o);
        }
    }

    // ゲームオーバー処理（自機メッシュとコライダーの無効化）
    void TriggerGameOver()
    {
        isGameActive = false;
        if (gameOverTextObj != null) gameOverTextObj.SetActive(true);
        if (gameClearTextObj != null) gameClearTextObj.SetActive(false);
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
    }
}