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
    public int targetBallsToSpawn = 6; // ⭐ HOW MANY TARGET BALLS TO SPAWN

    private int ballsSpawned = 0;
    private int targetBallsSpawned = 0;
    private float spawnTimer = 0f;
    public bool isSpawning = false;
    private float spawnInterval = 0f;
    //private string currentTargetID;
    //private int actualTargetCount = 0;
    private GameObject currentTargetPrefab; // ⭐ STORE THE PREFAB DIRECTLY

    private List<GameObject> spawnQueue = new List<GameObject>();
    // Events
    public System.Action<GameObject> OnBallSpawned;
    public System.Action OnSpawningComplete;
    public System.Action<int> OnTargetBallSpawned; // Passes current target count

    void Start()
    {
        spawnInterval = spawnDuration / totalBallsToSpawn;
    }
    void Update()
    {
        if (isSpawning && ballsSpawned < totalBallsToSpawn)
        {
            spawnTimer += Time.deltaTime;
            

            if (spawnTimer >= spawnInterval)
            {
                SpawnNextBall();
                //SpawnRandomBall();
                spawnTimer = 0f;
            }
        }
    }

    public void StartSpawning(GameObject targetPrefab)//string targetBallID)
    {
        //currentTargetID = targetBallID;
        currentTargetPrefab = targetPrefab;
        ballsSpawned = 0;
        targetBallsSpawned= 0;
        //actualTargetCount = 0;
        spawnTimer = 0f;
        isSpawning = true;

        CreateRandomSpawnQueue();

        Debug.Log($"Started spawning. Target: {targetPrefab.name}");


    }
    void CreateRandomSpawnQueue()
    {
        spawnQueue.Clear();

        // Create list with target balls
        for (int i = 0; i < targetBallsToSpawn; i++)
        {
            spawnQueue.Add(currentTargetPrefab);
        }

        // Create list with non-target balls
        int nonTargetBallsCount = totalBallsToSpawn - targetBallsToSpawn;
        for (int i = 0; i < nonTargetBallsCount; i++)
        {
            GameObject randomNonTarget = GetRandomNonTargetBall();
            spawnQueue.Add(randomNonTarget);
        }

        // ⭐ SHUFFLE THE QUEUE FOR RANDOM ORDER
        ShuffleQueue();

        Debug.Log($"Created spawn queue: {targetBallsToSpawn} target + {nonTargetBallsCount} non-target balls");
    }

    // ⭐ NEW: SHUFFLE USING FISHER-YATES ALGORITHM
    void ShuffleQueue()
    {
        for (int i = spawnQueue.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            GameObject temp = spawnQueue[i];
            spawnQueue[i] = spawnQueue[randomIndex];
            spawnQueue[randomIndex] = temp;
        }
    }

    // ⭐ MODIFIED: SPAWN FROM PRE-PLANNED QUEUE
    void SpawnNextBall()
    {
        if (spawnQueue.Count == 0 || ballsSpawned >= spawnQueue.Count)
        {
            Debug.LogError("Spawn queue is empty or invalid!");
            return;
        }


        // Get next ball from queue
        GameObject ballToSpawn = spawnQueue[ballsSpawned];
        if (ballToSpawn == null)
        {
            Debug.LogError($"Ball prefab at index {ballsSpawned} is NULL! Regenerating queue.");
            CreateRandomSpawnQueue(); // Regenerate if prefabs were destroyed
            return;
        }

        Vector3 spawnPos = transform.position +
                      new Vector3(Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                                  Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                                  0);

        GameObject newBall = Instantiate(ballToSpawn, spawnPos, Quaternion.identity);

        // ⭐ CHECK IF THIS IS A TARGET BALL
        if (ballToSpawn == currentTargetPrefab)
        {
            targetBallsSpawned++;
            OnTargetBallSpawned?.Invoke(targetBallsSpawned);
            Debug.Log($"Target ball spawned! {targetBallsSpawned}/{targetBallsToSpawn}");
        }

        ballsSpawned++;
        OnBallSpawned?.Invoke(newBall);

        if (ballsSpawned >= totalBallsToSpawn)
        {
            isSpawning = false;
            OnSpawningComplete?.Invoke();
            Debug.Log($"Spawning complete. Total balls: {ballsSpawned}, Target balls: {targetBallsSpawned}");
        }
    }


   /* void SpawnRandomBall()
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




        GameObject ballToSpawn;

        // ⭐ DECIDE: SPAWN TARGET BALL OR RANDOM BALL?
        if (targetBallsSpawned < targetBallsToSpawn &&
            ballsSpawned < totalBallsToSpawn - (targetBallsToSpawn - targetBallsSpawned))
        {
            // Spawn target ball if we haven't reached target count
            // AND there are enough balls left to spawn remaining targets
            ballToSpawn = currentTargetPrefab;
            targetBallsSpawned++;

            OnTargetBallSpawned?.Invoke(targetBallsSpawned);
            Debug.Log($"Target ball spawned! {targetBallsSpawned}/{targetBallsToSpawn}");
        }
        else
        {
            // Spawn random non-target ball
            ballToSpawn = GetRandomNonTargetBall();
        }

        GameObject newBall = Instantiate(ballToSpawn, spawnPos, Quaternion.identity);
        ballsSpawned++;
        OnBallSpawned?.Invoke(newBall);

        if (ballsSpawned >= totalBallsToSpawn)
        {
            isSpawning = false;
            OnSpawningComplete?.Invoke();
            Debug.Log($"Spawning complete. Total balls: {ballsSpawned}, Target balls: {targetBallsSpawned}");
        }
        *//*        // Random ball prefab
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
                }*//*
    }
*/
    // ⭐ NEW: GET RANDOM BALL THAT ISN'T THE TARGET
    GameObject GetRandomNonTargetBall()
    {
        List<GameObject> nonTargetPrefabs = new List<GameObject>();

        foreach (GameObject prefab in ballPrefabs)
        {
            if (prefab != currentTargetPrefab) // ⭐ SIMPLE COMPARISON!
            {
                nonTargetPrefabs.Add(prefab);
            }
        }

      
        return nonTargetPrefabs[Random.Range(0, nonTargetPrefabs.Count)];
        

    
    }

    public int GetActualTargetCount()
    {
        return targetBallsSpawned;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

}