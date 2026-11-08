using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class Comet3D : MonoBehaviour
{
    public float minSpeed = 20f;
    public float maxSpeed = 40f;
    private float speed;

    [Header("爆発エフェクト")]
    public GameObject explosionPrefab; // 作成した爆発プレハブを割り当てる枠

    [Header("サウンド設定")]
    public AudioClip explosionSE; // 追加: 爆発4の音声を割り当てる枠
    [Range(0f, 1f)] public float volume = 0.4f; // Inspectorで調整できるスライダー
    
    private Coroutine autoDespawnCoroutine;
    
    //  Start() ではなく OnEnable() に変更！
    // プールから再利用（アクティブ化）されるたびに毎回実行されます
    private void OnEnable()
    {
        speed = Random.Range(minSpeed, maxSpeed);

        // 3秒後に自動でプールに戻すコルーチンを開始
        autoDespawnCoroutine = StartCoroutine(AutoDespawnRoutine());
    }

    private void OnDisable()
    {
        // 非アクティブ化されたらタイマーを停止する（誤作動防止）
        if (autoDespawnCoroutine != null)
        {
            StopCoroutine(autoDespawnCoroutine);
        }
    }

    // 3秒経過したら自動回収する処理
    private IEnumerator AutoDespawnRoutine()
    {
        yield return new WaitForSeconds(3f);
        SimplePool.Despawn(gameObject);
    }
    
    void Update()
    {
        // 奥から手前（Z軸のマイナス方向）へ直進
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);
    }

    // 弾とぶつかった時の判定処理
    private void OnTriggerEnter(Collider other)
    {
        // ぶつかった相手が「Bullet」スクリプトを持っている（またはTagがBullet）場合
        if (other.GetComponent<Bullet>() != null || other.CompareTag("Bullet"))
        {
            // 爆発エフェクトを SimplePool から生成
            if (explosionPrefab != null)
            {
                SimplePool.Spawn(explosionPrefab, transform.position, transform.rotation);
                // 爆発エフェクト自身の消去は、AutoDespawnEffect スクリプト
            }

            // 爆発音（爆発4）を再生
            if (explosionSE != null)
            {
                AudioSource.PlayClipAtPoint(explosionSE, Camera.main.transform.position, volume);
            }


            // スコアを加算する
            PauseManager pauseManager = FindAnyObjectByType<PauseManager>();
            if (pauseManager != null)
            {
                pauseManager.AddScore(100); // 1匹につき100点
            }

            // 弾と彗星自身を Destroy ではなく SimplePool.Despawn でプールに戻す
            SimplePool.Despawn(other.gameObject); // 弾をプールに戻す
            SimplePool.Despawn(gameObject);       // 彗星自身をプールに戻す
        }
    }
}