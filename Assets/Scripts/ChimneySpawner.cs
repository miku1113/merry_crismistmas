using UnityEngine;
using System.Collections.Generic;

public class ChimneySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> housePrefabs; 
    // Replaces tree lists - Single type of tree now
    public GameObject treePrefab; 
    // NEW: Cloud Prefabs (Simple Clouds)
    public List<GameObject> cloudPrefabs;
    public GameObject darkCloudPrefab; // Explicit Dark Cloud reference
    
    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public int minCoins = 4;
    public int maxCoins = 6;
    public float coinSpacing = 1.5f;
    public float coinSpawnChance = 0.6f; // 60% chance to spawn coins in a gap


    public Transform santaTransform;
    
    [Header("Spawn Settings")]
    public float spawnRate = 5f;
    public float spawnDistanceAhead = 15f;
    public float chimneyY = -4f;
    public float treeY = -4.2f; 
    
    [Header("Cloud Settings")]
    public float minCloudY = 0f;
    public float maxCloudY = 4f;

    public float randomXOffset = 2f;

    private float nextSpawnTime;
    private int cloudSpawnCount = 0; // Track how many clouds have spawned

    void Start()
    {
        int difficulty = PlayerPrefs.GetInt("GameDifficulty", 1);
        if (difficulty == 0) spawnRate *= 1.2f; // Slower spawning
        else if (difficulty == 2) spawnRate *= 0.8f; // Faster spawning
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnObstacles();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnObstacles()
    {
        if (santaTransform == null) return;

        // Spawn Random House
        if (housePrefabs != null && housePrefabs.Count > 0)
        {
            float chimneyX = santaTransform.position.x + spawnDistanceAhead + Random.Range(-randomXOffset, randomXOffset);
            
            // Pick random house
            GameObject prefabToSpawn = housePrefabs[Random.Range(0, housePrefabs.Count)];
            Instantiate(prefabToSpawn, new Vector3(chimneyX, chimneyY, 0), Quaternion.identity);
            
            // Calculate Distance to next spawn roughly (Speed * Time)
            // Assuming Speed is approx 5, Distance is 25.
            // Halfway is 12.5.
            
            // Spawn Single Static Tree after the chimney (Ground Obstacle)
            if (treePrefab != null)
            {
                // Place tree safely in the middle-ish (towards the end of the gap)
                float treeX = chimneyX + 18f; 
                // Static Tree on Ground (No scaling or height randomization)
                Instantiate(treePrefab, new Vector3(treeX, treeY, 0), Quaternion.identity);
            }

            // Spawn a Random Cloud (Aerial Obstacle)
            if (cloudPrefabs != null && cloudPrefabs.Count > 0)
            {
                // Spawn cloud near the house
                float cloudX = chimneyX + Random.Range(-2f, 2f);
                float cloudY = Random.Range(minCloudY, maxCloudY);
                
                GameObject cloudToSpawn;

                // Check Cloud Counter
                cloudSpawnCount++;
                if (cloudSpawnCount >= 10 && darkCloudPrefab != null)
                {
                    cloudToSpawn = darkCloudPrefab;
                    cloudSpawnCount = 0; // Reset counter
                    Debug.Log("Spawning Dark Cloud!");
                }
                else
                {
                    cloudToSpawn = cloudPrefabs[Random.Range(0, cloudPrefabs.Count)];
                }

                GameObject cloud = Instantiate(cloudToSpawn, new Vector3(cloudX, cloudY, 0), Quaternion.identity);
                
                float randomScale = Random.Range(0.3f, 0.5f);
                cloud.transform.localScale = new Vector3(randomScale, randomScale, 1f);
            }

            // Spawn Coins (Line of 4-6 coins) with a random chance
            if (coinPrefab != null && Random.value < coinSpawnChance)
            {
                // Place coins in the empty space between House and Tree
                // House is X. Tree is X + 18.
                // Start coins at X + 4 (Clears house).
                // Max coins 6 * 1.5 spacing = 9 units width.
                // End at X + 13.
                // Clears Tree (at X + 18).
                
                float coinStartX = chimneyX + 4f; 
                int coinCount = Random.Range(minCoins, maxCoins + 1);
                
                // Random Height for the line
                float coinLineY = Random.Range(0f, 3f);

                for (int i = 0; i < coinCount; i++)
                {
                    Vector3 coinPos = new Vector3(coinStartX + (i * coinSpacing), coinLineY, 0);
                    Instantiate(coinPrefab, coinPos, Quaternion.identity);
                }
            }
        }
    }
}
