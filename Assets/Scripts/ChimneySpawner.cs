using UnityEngine;
using System.Collections.Generic;

public class ChimneySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> housePrefabs; 
    // Support for multiple tree types
    public List<GameObject> treePrefabs; 
    // NEW: Cloud Prefabs (Simple Clouds)
    public List<GameObject> cloudPrefabs;
    public GameObject darkCloudPrefab; // Explicit Dark Cloud reference
    
    [Header("Ravan Level Prefabs")]
    public GameObject yagnaPrefab;
    public bool isRavanLevel = false;

    [Header("Coin & Key Settings")]
    public GameObject coinPrefab;
    public int minCoins = 4;
    public int maxCoins = 6;
    public float coinSpacing = 1.5f;
    public float coinSpawnChance = 0.6f; 
    public GameObject keyPrefab;
    public float keySpawnChance = 0.05f; // 5% chance to spawn a key instead of a coin group


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
    private bool firstSpawnDone = false;

    void Start()
    {
        // Detect if we are in Ravan level (can be expanded to check LevelData)
        string selectedLevel = PlayerPrefs.GetString("SelectedLevel", "");
        if (selectedLevel.Contains("Ravan") || selectedLevel.Contains("Level 2"))
        {
            isRavanLevel = true;
        }

        Debug.Log($"[DIAGNOSTICS] ChimneySpawner - Final Spawn Rate: {spawnRate}, Ravan Level: {isRavanLevel}");
    }

    void Update()
    {
        // Safety check to prevent rapid spawning if rate is zero or negative
        if (spawnRate <= 0.1f) spawnRate = 5f; 

        if (Time.time >= nextSpawnTime)
        {
            SpawnObstacles();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnObstacles()
    {
        if (santaTransform == null) return;

        // Force First Spawn to be deterministic (Fixed House + Dark Cloud) ONLY for Tutorial
        if (!firstSpawnDone && PlayerPrefs.GetInt("TutorialComplete", 0) == 0)
        {
            SpawnFirstSetFixed();
            firstSpawnDone = true;
            return;
        }

        float spawnX = santaTransform.position.x + spawnDistanceAhead + Random.Range(-randomXOffset, randomXOffset);

        // Spawn Random House OR Yagna
        if (isRavanLevel)
        {
            if (yagnaPrefab != null)
            {
                Instantiate(yagnaPrefab, new Vector3(spawnX, chimneyY, 0), Quaternion.identity);
            }
        }
        else if (housePrefabs != null && housePrefabs.Count > 0)
        {
            // Pick random house
            GameObject prefabToSpawn = housePrefabs[Random.Range(0, housePrefabs.Count)];
            Instantiate(prefabToSpawn, new Vector3(spawnX, chimneyY, 0), Quaternion.identity);
        }
            
        // Calculate Distance to next spawn roughly (Speed * Time)
        // Assuming Speed is approx 5, Distance is 25.
        // Halfway is 12.5.
        
        // Spawn a random Tree exactly between this house and the expected next house
        if (treePrefabs != null && treePrefabs.Count > 0)
        {
            float moveSpeed = 5f; // Default speed
            if (santaTransform != null)
            {
                var santa = santaTransform.GetComponent<SantaController>();
                // Also check for other controllers if needed, but SantaController is safe if only Santa is used
                // Or generalized get component logic
                if (santa != null) moveSpeed = santa.moveSpeed;
                else
                {
                    var plane = santaTransform.GetComponent<PlaneController>();
                    if (plane != null) moveSpeed = plane.moveSpeed;
                    else
                    {
                        var witch = santaTransform.GetComponent<WitchController>();
                        if (witch != null) moveSpeed = witch.moveSpeed;
                    }
                }
            }
            
            // midPoint is half of the distance Santa travels between spawns
            float gapDistance = spawnRate * moveSpeed;
            float treeX = spawnX + (gapDistance / 2f);

            // Pick random tree
            GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Count)];

            // Instantiate tree at the calculated midpoint
            Instantiate(treePrefab, new Vector3(treeX, treeY, 0), Quaternion.identity);
        }

        // Spawn a Random Cloud (Aerial Obstacle)
        if (cloudPrefabs != null && cloudPrefabs.Count > 0)
        {
            // Spawn cloud near the house
            float cloudX = spawnX + Random.Range(-2f, 2f);
            float cloudY = Random.Range(minCloudY, maxCloudY);
            
            GameObject cloudToSpawn;

            // Randomize Dark Clouds (e.g., 20% chance)
            if (darkCloudPrefab != null && Random.value < 0.2f)
            {
                cloudToSpawn = darkCloudPrefab;
                Debug.Log("Spawning Random Dark Cloud!");
            }
            else
            {
                cloudToSpawn = cloudPrefabs[Random.Range(0, cloudPrefabs.Count)];
            }

            GameObject cloud = Instantiate(cloudToSpawn, new Vector3(cloudX, cloudY, 0), Quaternion.identity);
            
            float randomScale = Random.Range(0.3f, 0.5f);
            cloud.transform.localScale = new Vector3(randomScale, randomScale, 1f);
        }

        // Spawn Coins OR Key
        float randomVal = Random.value;
        if (keyPrefab != null && randomVal < keySpawnChance)
        {
            // Spawn a single key instead of coins
            float keyX = spawnX + 4f;
            float keyY = Random.Range(0f, 3f);
            Instantiate(keyPrefab, new Vector3(keyX, keyY, 0), Quaternion.identity);
            Debug.Log("Spawning Random Key!");
        }
        else if (coinPrefab != null && randomVal < coinSpawnChance)
        {
            float coinStartX = spawnX + 4f; 
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

    private void SpawnFirstSetFixed()
    {
        // Deterministic Spawn for Tutorial
        // House at fixed distance ahead
        float spawnX = santaTransform.position.x + spawnDistanceAhead; // e.g. +15f

        if (housePrefabs != null && housePrefabs.Count > 0)
        {
            GameObject prefabToSpawn = housePrefabs[0]; // Use first house for consistency
            Instantiate(prefabToSpawn, new Vector3(spawnX, chimneyY, 0), Quaternion.identity);
        }

        // GUARANTEED Dark Cloud just before the house
        // So user sees it coming
        if (darkCloudPrefab != null)
        {
            float cloudX = spawnX - 5f; // 5 units before the house
            float cloudY = (minCloudY + maxCloudY) / 2f; // Mid-height
            
            Instantiate(darkCloudPrefab, new Vector3(cloudX, cloudY, 0), Quaternion.identity);
            Debug.Log("Spawning Guaranteed First Dark Cloud!");
        }

        Debug.Log("First Set Spawned Deterministically.");
    }
}
