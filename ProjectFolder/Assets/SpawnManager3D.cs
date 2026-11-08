using UnityEngine; // これがないと動かないので一応書いておきます

public class CometSpawner : MonoBehaviour // ← クラス名はあなたのファイル名に合わせてください！
{
    public GameObject cometPrefab; // ここにProjectウィンドウの蒼いプレハブを入れる。
    public float spawnRate = 0.2f; // 生成間隔(秒)
    public float spawnDepth = 60f; // 出現する奥の距離

    private float timer = 0f;

    private void Update()
    {
        // ゲームが一時停止中（Time.timeScale が 0）の時は、これ以上処理をしない（軽量化）
        if (Time.timeScale <= 0f)
        {
            return;
        }

        // 毎フレーム、タイマーを進める
        timer += Time.deltaTime;

        // 指定した時間（spawnRate）が経つたびに生成
        if (timer >= spawnRate)
        {
            // timer = 0f; だと、ズレが蓄積していくため、spawnRate分だけ引くのがより正確です
            timer -= spawnRate;

            // プレイヤーの視界に入る範囲（X:左右, Y:上下）にランダム配置
            float randomX = Random.Range(-15f, 15f);
            float randomY = Random.Range(-10f, 10f);
            Vector3 spawnPos = new Vector3(randomX, randomY, spawnDepth);

            // 生成
            SimplePool.Spawn(cometPrefab, spawnPos, Quaternion.identity);
        }
    }
}