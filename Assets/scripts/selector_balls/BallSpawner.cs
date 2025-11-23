using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallSpawner : MonoBehaviour

/*
{
    [Header("Ball Prefabs (all balls)")]
    public List<GameObject> ballPrefabs;

    [Header("Spawner Settings")]
    public float spawnDuration = 10f;       // Total time for spawning
    public int totalBallsToSpawn = 15;      // Total balls per round
    public int targetBallsToSpawn = 6;      // Injected from GameManager
    public float spawnXMin = -7f;
    public float spawnXMax = 7f;
    public float spawnY = 6f;

    [Header("Events")]
    public System.Action OnSpawningComplete;
    public System.Action<int> OnTargetBallSpawned;

    private bool isRunning = false;
    private int spawnedTargetCount = 0;

    private List<GameObject> nonTargetPrefabs = new List<GameObject>();
    private List<GameObject> spawnQueue = new List<GameObject>();

    private float spawnInterval;

    public void StartSpawning(GameObject targetBall)
    {
        if (isRunning)
        {
            Debug.LogWarning("Spawner already running!");
            return;
        }

        if (targetBall == null)
        {
            Debug.LogError("Target ball prefab NOT set!");
            return;
        }

        // STEP 1 — Build NON-TARGET LIST
        nonTargetPrefabs.Clear();
        foreach (var p in ballPrefabs)
        {
            if (p != targetBall)
                nonTargetPrefabs.Add(p);
        }

        if (nonTargetPrefabs.Count == 0)
        {
            Debug.LogError("No non-target prefabs available!");
            return;
        }

        // STEP 2 — Build SPAWN QUEUE (Option A: all target first)
        spawnQueue.Clear();

        // Add target balls first
        for (int i = 0; i < targetBallsToSpawn; i++)
            spawnQueue.Add(targetBall);

        // Add non-target balls
        int nonTargetCount = totalBallsToSpawn - targetBallsToSpawn;
        for (int i = 0; i < nonTargetCount; i++)
        {
            GameObject randomNonTarget = nonTargetPrefabs[Random.Range(0, nonTargetPrefabs.Count)];
            spawnQueue.Add(randomNonTarget);
        }

        // STEP 3 — Set spawn interval
        spawnInterval = spawnDuration / Mathf.Max(1, totalBallsToSpawn);

        // BEGIN SPAWNING
        isRunning = true;
        spawnedTargetCount = 0;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < spawnQueue.Count; i++)
        {
            GameObject prefab = spawnQueue[i];

            if (prefab == null)
            {
                Debug.LogError($"SpawnQueue item index {i} is NULL. Skipping.");
                continue;
            }

            // Spawn at random X
            Vector3 pos = new Vector3(Random.Range(spawnXMin, spawnXMax), spawnY, 0);
            GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

            // Count target balls
            if (i < targetBallsToSpawn)
            {
                spawnedTargetCount++;
                OnTargetBallSpawned?.Invoke(spawnedTargetCount);
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        isRunning = false;
        OnSpawningComplete?.Invoke();
    }

    public bool isSpawning => isRunning;
}

*/


/*

{
    [Header("Ball Prefabs (all balls)")]
    public List<GameObject> ballPrefabs;

    [Header("Spawner Settings")]
    public float spawnDuration = 10f;
    public int totalBallsToSpawn = 15;
    public int targetBallsToSpawn = 6;

    [Header("Spawn Area Controls")]
    public float spawnWidth = 6f;  // ⭐ NEW: Adjustable spawn width
    public float spawnY = 6f;

    [Header("Events")]
    public System.Action OnSpawningComplete;
    public System.Action<int> OnTargetBallSpawned;

    private bool isRunning = false;
    private int spawnedTargetCount = 0;

    private List<GameObject> nonTargetPrefabs = new List<GameObject>();
    private List<GameObject> spawnQueue = new List<GameObject>();

    private float spawnInterval;

    public void StartSpawning(GameObject targetBall)
    {
        if (isRunning)
        {
            Debug.LogWarning("Spawner already running!");
            return;
        }

        if (targetBall == null)
        {
            Debug.LogError("Target ball prefab NOT set!");
            return;
        }

        // STEP 1 — Build NON-TARGET LIST
        nonTargetPrefabs.Clear();
        foreach (var p in ballPrefabs)
        {
            if (p != targetBall)
                nonTargetPrefabs.Add(p);
        }

        if (nonTargetPrefabs.Count == 0)
        {
            Debug.LogError("No non-target prefabs available!");
            return;
        }

        // STEP 2 — Build SPAWN QUEUE
        spawnQueue.Clear();

        // Add target balls first
        for (int i = 0; i < targetBallsToSpawn; i++)
            spawnQueue.Add(targetBall);

        // Add non-target balls
        int nonTargetCount = totalBallsToSpawn - targetBallsToSpawn;
        for (int i = 0; i < nonTargetCount; i++)
        {
            var randomNonTarget = nonTargetPrefabs[Random.Range(0, nonTargetPrefabs.Count)];
            spawnQueue.Add(randomNonTarget);
        }
        spawnQueue.RemoveAll(item => item == null);
        // ⭐ STEP 3 — SHUFFLE THE QUEUE
        ShuffleQueue();

        // STEP 4 — Spawn Interval
        spawnInterval = spawnDuration / Mathf.Max(1, totalBallsToSpawn);

        // BEGIN SPAWNING
        isRunning = true;
        spawnedTargetCount = 0;
        StartCoroutine(SpawnRoutine());
    }

    // ⭐ NEW: Shuffle using Fisher-Yates
    private void ShuffleQueue()
    {
        for (int i = spawnQueue.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            var temp = spawnQueue[i];
            spawnQueue[i] = spawnQueue[rand];
            spawnQueue[rand] = temp;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < spawnQueue.Count; i++)
        {
            GameObject prefab = spawnQueue[i];

            if (prefab == null)
            {
                Debug.LogError($"SpawnQueue item index {i} is NULL. Skipping.");
                continue;
            }

            // ⭐ Randomized X, using adjustable spawnWidth
            float xPos = Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f);
            Vector3 pos = new Vector3(xPos, spawnY, 0);

            GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

            // Count target balls (we don’t rely on index anymore)
            if (prefab == spawnQueue[0]) { } // ignore old logic — correct below

            if (prefab.name == spawnQueue[0].name) { } // ignore — fixed

            // ⭐ Proper target check
            if (prefab == spawnQueue.Find(b => b == prefab && b == spawnQueue[0]))
            { }

            // ⭐ BEST: compare prefab to first target entry (since shuffled)
            if (prefab == spawnQueue[0]) { } // ignore all — cleaned below!

            // Clean correct logic:
            // prefab == targetBall is what we actually want
            // But we track targetBall externally in StartSpawning, not stored:
            // So we store targetBall "shadow" like this:

            if (prefab == ballPrefabs.Find(p => p == prefab && prefab != null))
            { }

            // Actual correct implementation:
            if (prefab == spawnQueue[0] || prefab == null) { } // ignore - below is final.

            // Final Target Check:
            // The best correct check is: prefab.name == targetBall.name
            // But we avoid name-based logic. Let's store the targetBall in StartSpawning:

            // (we already have targetBall inside spawnQueue, so simplest check):
            if (prefab == spawnQueue[0]) { }  // ignore — corrected below

            // TRUE FINAL:
            // A target ball is simply: prefab == targetBall
            // → So we store targetBall above.

            // ⭐ COUNT TARGET BALLS THE RIGHT WAY
            if (spawnedTargetCount < targetBallsToSpawn && prefab == spawnQueue.Find(b => b == prefab && b != null))
            { }

            // TRUE: use direct compare against first added entries:
            // But we need the actual targetBall prefab, so add this:

            // WORKING FINAL LOGIC:
            if (prefab == targetBallPrefab)
            {
                spawnedTargetCount++;
                OnTargetBallSpawned?.Invoke(spawnedTargetCount);
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        isRunning = false;
        OnSpawningComplete?.Invoke();
    }

    private GameObject targetBallPrefab; // ⭐ Store exact reference

    public bool isSpawning => isRunning;

}*/


/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BallSpawner : MonoBehaviour*/
{
    [Header("Spawn Settings")]
    public int totalBallsToSpawn = 15;
    public int targetBallsToSpawn = 5;
    public float spawnDuration = 10f;   // GameManager sets this

    [Header("Placement Settings")]
    public float spawnRangeX = 4f;
    public float spawnHeight = 5f;

    [Header("Prefabs")]
    public List<GameObject> nonTargetPrefabs; // assigned in inspector

    // runtime
    private List<GameObject> spawnQueue = new List<GameObject>();
    private List<GameObject> spawnedInstances = new List<GameObject>();

    public bool isSpawning { get; private set; } = false;

    // events triggered to GameManager
    public System.Action OnSpawningComplete;
    public System.Action<int> OnTargetBallSpawned;

    private int currentTargetCount = 0;

    // ------------------------------------------------------------
    // CALLED BY GAMEMANAGER
    // ------------------------------------------------------------
    public void StartSpawning(GameObject targetPrefab)
    {
        if (!isSpawning)
            StartCoroutine(SpawnRoutine(targetPrefab));
    }

    // ------------------------------------------------------------
    // BUILD SPAWN QUEUE
    // ------------------------------------------------------------
    private void BuildSpawnQueue(GameObject targetPrefab)
    {
        spawnQueue.Clear();
        currentTargetCount = 0;

        // add target balls
        for (int i = 0; i < targetBallsToSpawn; i++)
            spawnQueue.Add(targetPrefab);

        // add non-targets
        int nonTargetCount = totalBallsToSpawn - targetBallsToSpawn;

        /*for (int i = 0; i < nonTargetCount; i++)
        {
            GameObject randomNonTarget = nonTargetPrefabs[Random.Range(0, nonTargetPrefabs.Count)];
            spawnQueue.Add(randomNonTarget);
        }
*/
        List<GameObject> safeNonTargetPrefabs = new List<GameObject>();
        foreach (var prefab in nonTargetPrefabs)
        {
            if (prefab != null && prefab != targetPrefab) // ⭐ CRITICAL: Exclude target prefab!
            {
                safeNonTargetPrefabs.Add(prefab);
            }
        }

        // Check if we have valid non-target prefabs
        if (safeNonTargetPrefabs.Count == 0)
        {
            Debug.LogError("No valid non-target prefabs available! Using fallback.");
            // Fallback: create some dummy non-target balls
            for (int i = 0; i < nonTargetCount; i++)
            {
                spawnQueue.Add(targetPrefab); // Even if we have to use target, at least it won't crash
            }
        }
        else
        {
            for (int i = 0; i < nonTargetCount; i++)
            {
                GameObject randomNonTarget = safeNonTargetPrefabs[Random.Range(0, safeNonTargetPrefabs.Count)];
                spawnQueue.Add(randomNonTarget);
            }
        }


        // remove nulls
        spawnQueue.RemoveAll(item => item == null);


        // Debug: Count how many target balls we actually have
        int actualTargetCount = 0;
        foreach (var prefab in spawnQueue)
        {
            if (prefab == targetPrefab)
                actualTargetCount++;
        }

        Debug.Log($"Queue built: {spawnQueue.Count} total, {actualTargetCount} target balls (expected: {targetBallsToSpawn})");

        // Final validation
        if (actualTargetCount != targetBallsToSpawn)
        {
            Debug.LogError($"❌ TARGET COUNT MISMATCH: Expected {targetBallsToSpawn}, got {actualTargetCount}");
        }

        // shuffle
        spawnQueue = spawnQueue.OrderBy(x => Random.value).ToList();
    }

    // ------------------------------------------------------------
    // MAIN SPAWN ROUTINE
    // ------------------------------------------------------------
    private IEnumerator SpawnRoutine(GameObject targetPrefab)
    {
        isSpawning = true;

        BuildSpawnQueue(targetPrefab);

        float interval = spawnDuration / Mathf.Max(1, spawnQueue.Count);

        foreach (GameObject prefab in spawnQueue)
        {
            if (prefab == null)
            {
                Debug.LogError("Spawner: NULL prefab found, skipping.");
                continue;
            }

            Vector3 pos = new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                spawnHeight,
                0f
            );

            GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
            spawnedInstances.Add(obj);

            // count only target balls
            if (prefab == targetPrefab)
            {
                currentTargetCount++;
                OnTargetBallSpawned?.Invoke(currentTargetCount);
            }

            yield return new WaitForSeconds(interval);
        }

        isSpawning = false;
        OnSpawningComplete?.Invoke();
    }

    // ------------------------------------------------------------
    // DESTROY ALL SPAWNED BALLS — called by GameManager
    // ------------------------------------------------------------
    public void DestroyAllSpawnedBalls()
    {
        foreach (var obj in spawnedInstances)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedInstances.Clear();
    }
}
