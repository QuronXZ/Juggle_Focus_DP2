using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager_Counting : MonoBehaviour
{
    [Header("References")]
    public BallSpawner ballSpawner;
    public TextMeshProUGUI targetDisplay;
    public TextMeshProUGUI timerDisplay;
    public TextMeshProUGUI resultDisplay;
    public TMP_InputField countInputField;
    public GameObject gamePanel;
    public GameObject resultPanel;

    [Header("Target Display")]
    public Transform targetDisplaySpawnPoint; // Where to show the target ball
    public Vector3 targetBallScale = new Vector3(2f, 2f, 2f); // Larger for visibility

    [Header("Game Settings")]
    public int ballsPerRound = 15;
    public float roundDuration = 10f;

    private GameObject currentTargetBallPrefab;
    private GameObject displayedTargetBall;
    private int actualTargetCount = 0;
    private int playerCount = 0;
    private float gameTimer = 0f;
    private bool gameActive = false;

    void Start()
    {
        // Subscribe to spawner events
        ballSpawner.OnSpawningComplete += OnSpawningComplete;
        ballSpawner.OnTargetBallSpawned += OnTargetBallSpawned;

        ShowResultPanel(false);
    }

    public void StartNewRound()
    {
        // Clean up previous target display
        if (displayedTargetBall != null)
            Destroy(displayedTargetBall);

        // Reset game state
        actualTargetCount = 0;
        playerCount = 0;
        gameTimer = 0f;
        gameActive = true;

        // Choose random target from available prefabs
        currentTargetBallPrefab = ballSpawner.ballPrefabs[Random.Range(0, ballSpawner.ballPrefabs.Count)];
        BallController targetController = currentTargetBallPrefab.GetComponent<BallController>();
        string targetID = targetController.ballID;

        // Display the target ball
        DisplayTargetBall();

        // Update UI
        UpdateTargetDisplay(targetController);
        countInputField.text = "";
        countInputField.interactable = true;
        ShowResultPanel(false);

        // Start spawning
        ballSpawner.totalBallsToSpawn = ballsPerRound;
        ballSpawner.spawnDuration = roundDuration;
        ballSpawner.StartSpawning(targetID);

        Debug.Log($"New round started. Target: {targetID}");
    }

    void Update()
    {
        if (gameActive)
        {
            gameTimer += Time.deltaTime;
            UpdateTimerDisplay();

            // Auto-end when time's up
            if (gameTimer >= roundDuration)
            {
                OnSpawningComplete();
            }
        }
    }

    void DisplayTargetBall()
    {
        if (currentTargetBallPrefab != null && targetDisplaySpawnPoint != null)
        {
            displayedTargetBall = Instantiate(currentTargetBallPrefab, targetDisplaySpawnPoint.position, Quaternion.identity);
            displayedTargetBall.transform.localScale = targetBallScale;

            // Disable movement for display ball
            BallController controller = displayedTargetBall.GetComponent<BallController>();
            if (controller != null)
                controller.enabled = false;

            // Make it static
            Rigidbody rb = displayedTargetBall.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;
        }
    }

    void OnTargetBallSpawned(int currentCount)
    {
        actualTargetCount = currentCount;
    }

    void OnSpawningComplete()
    {
        gameActive = false;
        countInputField.interactable = false;
        Debug.Log($"Round complete. Waiting for player input. Actual count: {actualTargetCount}");
    }

    public void SubmitCount()
    {
        if (int.TryParse(countInputField.text, out playerCount))
        {
            EvaluateResults();
        }
        else
        {
            resultDisplay.text = "Please enter a valid number!";
            ShowResultPanel(true);
        }
    }

    void EvaluateResults()
    {
        ShowResultPanel(true);

        BallController targetController = currentTargetBallPrefab.GetComponent<BallController>();

        string resultText = $"<size=24><b>Round Complete!</b></size>\n\n";
        resultText += $"<b>Target:</b> {targetController.ballColor} {targetController.ballPattern}\n";
        resultText += $"<b>Your Count:</b> {playerCount}\n";
        resultText += $"<b>Actual Count:</b> {actualTargetCount}\n\n";

        int difference = Mathf.Abs(playerCount - actualTargetCount);

        if (difference == 0)
        {
            resultText += $"<color=green>✅ PERFECT! Excellent attention!</color>";
        }
        else if (difference <= 1)
        {
            resultText += $"<color=yellow>⚠️ Close! Difference: {difference}</color>";
        }
        else if (difference <= 2)
        {
            resultText += $"<color=orange>❌ Good effort. Difference: {difference}</color>";
        }
        else
        {
            resultText += $"<color=red>❌ Needs practice. Difference: {difference}</color>";
        }

        resultDisplay.text = resultText;
    }

    void UpdateTargetDisplay(BallController target)
    {
        if (targetDisplay != null)
        {
            targetDisplay.text = $"Count how many:\n<size=36><b>{target.ballColor} {target.ballPattern}</b></size>\nballs appear below";
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerDisplay != null)
        {
            float timeLeft = Mathf.Max(0f, roundDuration - gameTimer);
            timerDisplay.text = $"Time: {timeLeft:F1}s";

            // Color change when time is running out
            if (timeLeft < 3f)
                timerDisplay.color = Color.red;
            else
                timerDisplay.color = Color.white;
        }
    }

    void ShowResultPanel(bool show)
    {
        if (gamePanel != null) gamePanel.SetActive(!show);
        if (resultPanel != null) resultPanel.SetActive(show);
    }

    public void OnPlayAgainClicked()
    {
        StartNewRound();
    }
}