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
    public int crossCount = 10;
    public int batteryCount = 10;

    [Header("Map範囲")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    void Start()
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

        // レアアイテム
        SpawnRandomItem(tirePrefab);
        SpawnRandomItem(coalPrefab);
        SpawnRandomItem(driverPrefab);
        SpawnRandomItem(keyPrefab);
    }

    void SpawnRandomItem(GameObject itemPrefab)
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(minX, maxX),
            1f,
            Random.Range(minZ, maxZ)
        );

        Instantiate(
            itemPrefab,
            randomPosition,
            Quaternion.identity
        );
    }
}