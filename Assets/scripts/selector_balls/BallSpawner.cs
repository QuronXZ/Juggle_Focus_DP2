using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Ball Prefabs")]
    public List<GameObject> ballPrefabs = new List<GameObject>(); // Assign all 16 prefabs here

    [Header("Spawn Settings")]
    public int totalBallsToSpawn = 15;
    public float spawnDuration = 10f;
    public Vector3 spawnAreaSize = new Vector3(8f, 0f, 0f);

    private int ballsSpawned = 0;
    private float spawnTimer = 0f;
    private bool isSpawning = false;
    private string currentTargetID;
    private int actualTargetCount = 0;

    // Events
    public System.Action<GameObject> OnBallSpawned;
    public System.Action OnSpawningComplete;
    public System.Action<int> OnTargetBallSpawned; // Passes current target count

    void Update()
    {
        if (isSpawning && ballsSpawned < totalBallsToSpawn)
        {
            spawnTimer += Time.deltaTime;
            float spawnInterval = spawnDuration / totalBallsToSpawn;

            if (spawnTimer >= spawnInterval)
            {
                SpawnRandomBall();
                spawnTimer = 0f;
            }
        }
    }

    public void StartSpawning(string targetBallID)
    {
        currentTargetID = targetBallID;
        ballsSpawned = 0;
        actualTargetCount = 0;
        spawnTimer = 0f;
        isSpawning = true;
        Debug.Log($"Started spawning. Target: {targetBallID}");
    }

    void SpawnRandomBall()
    {
        if (ballPrefabs.Count == 0)
        {
            Debug.Log("Count is zero");
            return;
        }

            // Random spawn position
            Vector3 spawnPos = transform.position +
                          new Vector3(Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                                      Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                                      0);

        // Random ball prefab
        GameObject randomPrefab = ballPrefabs[Random.Range(0, ballPrefabs.Count)];
        GameObject newBall = Instantiate(randomPrefab, spawnPos, Quaternion.identity);

        // Check if this is a target ball
        BallController ballController = newBall.GetComponent<BallController>();
        if (ballController != null && ballController.MatchesTargetID(currentTargetID))
        {
            actualTargetCount++;
            OnTargetBallSpawned?.Invoke(actualTargetCount);
            Debug.Log($"Target ball spawned! Total: {actualTargetCount}");
        }

        ballsSpawned++;
        OnBallSpawned?.Invoke(newBall);

        if (ballsSpawned >= totalBallsToSpawn)
        {
            isSpawning = false;
            OnSpawningComplete?.Invoke();
            Debug.Log($"Spawning complete. Total target balls: {actualTargetCount}");
        }
    }

    public int GetActualTargetCount()
    {
        return actualTargetCount;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }
}