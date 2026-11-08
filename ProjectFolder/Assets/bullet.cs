using UnityEngine;
using UnityEngine.Rendering;

public class Bullet : MonoBehaviour
{
    public float speed = 30f; // 弾の速度（自機より少し速めが良い）
    public GameObject hitExplosionPrefab; // もしあれば、障害物に当たった時の小さな爆発エフェクト

    [Header("サウンド設定")]
    public AudioClip shootSE; // 追加: ビーム砲1の音声を割り当てる枠
    [Range(0f, 1f)] public float volume = 0.3f; // Inspectorで調整できるスライダー
    //  Start() ではなく OnEnable() に変更
    // プールから再利用（アクティブ化）されるたびに音が鳴るようになります
    private void OnEnable()
    {
        if (shootSE != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(shootSE, Camera.main.transform.position, volume);
        }
    }
    void Update()
    {
        // 前方（+Z方向）に移動させる
        // ゲーム画面で言うと「奥方向」へ飛んでいきます
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // 画面外（奥）に行き過ぎたら自動で削除する（メモリ節約のため）
        // とりあえずZ位置が50を超えたら削除するようにします
        if (transform.position.z > 50f)
        {
            SimplePool.Despawn(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 障害物（タグがObstacleのもの）に当たったら
        if (other.CompareTag("Obstacle"))
        {
            // ヒット時のエフェクトを生成（もし設定されていれば）
            if (hitExplosionPrefab != null)
            {
                SimplePool.Spawn(hitExplosionPrefab, transform.position, Quaternion.identity);
            }

            // 障害物をプールに戻す(変更前: Destroy)
            SimplePool.Despawn(other.gameObject);

            // 弾自身もプールに戻す(変更前: Destroy)
            SimplePool.Despawn(gameObject);

            
        }
    }
}
