using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject crossPrefab;
    public GameObject batteryPrefab;
    public GameObject tirePrefab;
    public GameObject coalPrefab;
    public GameObject driverPrefab;
    public GameObject keyPrefab;

    [Header("生成数")]
    public int crossCount = 5;
    public int batteryCount = 5;

    [Header("ランダムスポーン位置 (バッテリーと十字架用)")]
    public Transform[] randomSpawnPoints;

    [Header("特定アイテムのスポーン位置")]
    public Transform tireSpawnPoint;
    public Transform coalSpawnPoint;
    public Transform driverSpawnPoint;
    public Transform keySpawnPoint;

    void Start()
    {
        // ランダムスポーン地点が設定されているか確認
        if (randomSpawnPoints != null && randomSpawnPoints.Length > 0)
        {
            // cross
            for (int i = 0; i < crossCount; i++)
            {
                SpawnRandomItem(crossPrefab);
            }

            // battery
            for (int i = 0; i < batteryCount; i++)
            {
                SpawnRandomItem(batteryPrefab);
            }
        }
        else
        {
            Debug.LogWarning("Random Spawn Pointsが設定されていません。");
        }

        // レアアイテム（特定の位置）
        SpawnSpecificItem(tirePrefab, tireSpawnPoint);
        SpawnSpecificItem(coalPrefab, coalSpawnPoint);
        SpawnSpecificItem(driverPrefab, driverSpawnPoint);
        SpawnSpecificItem(keyPrefab, keySpawnPoint);
    }

    void SpawnRandomItem(GameObject itemPrefab)
    {
        // 10個など設定されたランダムスポーン位置から一つを選ぶ
        int randomIndex = Random.Range(0, randomSpawnPoints.Length);
        Transform spawnPoint = randomSpawnPoints[randomIndex];

        if (spawnPoint != null)
        {
            Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    void SpawnSpecificItem(GameObject itemPrefab, Transform spawnPoint)
    {
        // 指定された位置が設定されているか確認して生成
        if (spawnPoint != null)
        {
            Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogWarning(itemPrefab.name + " のスポーン位置が設定されていません。");
        }
    }
}