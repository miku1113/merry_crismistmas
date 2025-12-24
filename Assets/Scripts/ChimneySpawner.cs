using UnityEngine;

public class ChimneySpawner : MonoBehaviour
{
    public GameObject chimneyPrefab;
    public GameObject treePrefab; // New tree prefab
    public Transform santaTransform;
    
    [Header("Spawn Settings")]
    public float spawnRate = 2f;
    public float spawnDistanceAhead = 15f;
    public float chimneyY = -4f;
    public float treeY = -4.2f; // Trees might be slightly lower
    public float randomXOffset = 2f;

    private float nextSpawnTime;

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

        // Spawn Chimney
        if (chimneyPrefab != null)
        {
            float chimneyX = santaTransform.position.x + spawnDistanceAhead + Random.Range(-randomXOffset, randomXOffset);
            Instantiate(chimneyPrefab, new Vector3(chimneyX, chimneyY, 0), Quaternion.identity);

            // Spawn a Tree after the chimney (simulating "between" houses)
            if (treePrefab != null)
            {
                float treeX = chimneyX + (spawnRate * 5f * 0.5f); // Rough estimate of half the distance to next chimney
                Instantiate(treePrefab, new Vector3(treeX, treeY, 0), Quaternion.identity);
            }
        }
    }
}
