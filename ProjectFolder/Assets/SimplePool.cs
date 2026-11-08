using System.Collections.Generic;
using UnityEngine;

public static class SimplePool
{
    private static Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private static Dictionary<GameObject, GameObject> instanceToPrefabMap = new Dictionary<GameObject, GameObject>();

    // Instantiate の代わりに使う
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        GameObject obj = null;

        // 安全ガード：プールの中に既に Destroy されたオブジェクトが混ざっている場合、有効なものが見つかるまでスキップする
        while (poolDictionary[prefab].Count > 0)
        {
            obj = poolDictionary[prefab].Dequeue();
            if (obj != null) // 壊れていなければ採用！
            {
                break;
            }
        }

        // プールに使えるものが無ければ新しく生成
        if (obj == null)
        {
            obj = Object.Instantiate(prefab, position, rotation);
            instanceToPrefabMap[obj] = prefab;
        }
        else
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
        }

        return obj;
    }

    // Destroy の代わりに使う
    public static void Despawn(GameObject obj)
    {
        if (obj == null) return;

        if (instanceToPrefabMap.TryGetValue(obj, out GameObject prefab))
        {
            obj.SetActive(false);
            poolDictionary[prefab].Enqueue(obj);
        }
        else
        {
            Object.Destroy(obj);
        }
    }
}