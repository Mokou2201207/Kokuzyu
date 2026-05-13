using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("生成するPrefab")]
    public GameObject crossPrefab;
    public GameObject batteryPrefab;

    [Header("スポーンポイント")]
    public Transform[] spawnPoints;

    [Header("生成数")]
    public int crossCount = 10;
    public int batteryCount = 10;

    void Start()
    {
        // スポーンポイントをコピー
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        // cross生成
        for (int i = 0; i < crossCount; i++)
        {
            SpawnItem(crossPrefab, availablePoints);
        }

        // battery生成
        for (int i = 0; i < batteryCount; i++)
        {
            SpawnItem(batteryPrefab, availablePoints);
        }
    }

    void SpawnItem(GameObject itemPrefab, List<Transform> points)
    {
        // スポーンポイント不足チェック
        if (points.Count == 0)
        {
            Debug.LogWarning("スポーンポイントが足りません");
            return;
        }

        // ランダムな場所を選択
        int randomIndex = Random.Range(0, points.Count);

        Transform spawnPoint = points[randomIndex];

        // アイテム生成
        Instantiate(
            itemPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // 同じ場所を使わないよう削除
        points.RemoveAt(randomIndex);
    }
}