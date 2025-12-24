using UnityEngine;

public class SnowfallManager : MonoBehaviour
{
    public GameObject snowBallPrefab;
    public float spawnRate = 0.5f;
    public float spawnRangeX = 15f;
    public float spawnHeight = 10f;
    
    private Transform mainCameraTransform;
    private float nextSpawnTime;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime && snowBallPrefab != null)
        {
            SpawnSnowBall();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnSnowBall()
    {
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPos = new Vector3(mainCameraTransform.position.x + randomX, spawnHeight, 5f); // 5f is background Z
        Instantiate(snowBallPrefab, spawnPos, Quaternion.identity);
    }
}
