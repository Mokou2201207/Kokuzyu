using UnityEngine;
using System.Collections.Generic;

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

    private List<Transform> availableSpawnPoints = new List<Transform>();

    void Start()
    {
        // ランダムスポーン地点が設定されているか確認
        if (randomSpawnPoints != null && randomSpawnPoints.Length > 0)
        {
            availableSpawnPoints = new List<Transform>(randomSpawnPoints);

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
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogWarning("スポーン可能な位置が足りません（アイテムの合計数がスポーン位置の数を超えています）。");
            return;
        }

        // 利用可能なスポーン位置からランダムに一つを選ぶ
        int randomIndex = Random.Range(0, availableSpawnPoints.Count);
        Transform spawnPoint = availableSpawnPoints[randomIndex];

        if (spawnPoint != null)
        {
            Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        // 選択された位置をリストから削除し、他のアイテムと被らないようにする
        availableSpawnPoints.RemoveAt(randomIndex);
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