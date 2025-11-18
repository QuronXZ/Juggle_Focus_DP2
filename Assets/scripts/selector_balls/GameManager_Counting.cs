using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class GameManager_Counting : MonoBehaviour
{
    [Header("References")]
    public BallSpawner ballSpawner;
    public TextMeshProUGUI targetDisplay;
    public TextMeshProUGUI timerDisplay;
    public TextMeshProUGUI resultDisplay;
    //public TMP_InputField countInputField;
    public GameObject gamePanel;
    public GameObject resultPanel;

    [Header("Answer Buttons")]
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public TextMeshProUGUI button1Text;
    public TextMeshProUGUI button2Text;
    public TextMeshProUGUI button3Text;

    [Header("Target Display")]
    public Transform targetDisplaySpawnPoint; // Where to show the target ball
    public Vector3 targetBallScale = new Vector3(2f, 2f, 2f); // Larger for visibility

    [Header("Game Settings")]
    public GameObject targetBallPrefab; // ⭐
    public int ballsPerRound = 15;
    public float roundDuration = 10f;
    public int targetBallsToSpawn = 6; // ⭐

    private GameObject currentTargetBallPrefab;
    private GameObject displayedTargetBall;
    private int actualTargetCount = 0;
    private int correctAnswerIndex = 0;
    //private int playerCount = 0;
    private float gameTimer = 0f;
    private bool gameActive = false;
    private bool waitingForAnswer = false;

    void Start()
    {
        // Subscribe to spawner events
        ballSpawner.OnSpawningComplete += OnSpawningComplete;
        ballSpawner.OnTargetBallSpawned += OnTargetBallSpawned;

        // Set up button listeners
        answerButton1.onClick.AddListener(() => OnAnswerSelected(0));
        answerButton2.onClick.AddListener(() => OnAnswerSelected(1));
        answerButton3.onClick.AddListener(() => OnAnswerSelected(2));
        SetAnswerButtonsActive(false);

        ShowResultPanel(false);
        StartNewRound();
    }

    public void StartNewRound()
    {
        // Clean up previous target display
        if (displayedTargetBall != null)
            Destroy(displayedTargetBall);

        // Reset game state
        actualTargetCount = 0;
        //playerCount = 0;
        gameTimer = 0f;
        gameActive = true;
        waitingForAnswer = false;

        /*        // Choose random target from available prefabs
                currentTargetBallPrefab = ballSpawner.ballPrefabs[Random.Range(0, ballSpawner.ballPrefabs.Count)];
                BallController targetController = currentTargetBallPrefab.GetComponent<BallController>();
                string targetID = targetController.ballID;
        */
        // Display the target ball
        DisplayTargetBall();

        /*   // Update UI
           UpdateTargetDisplay(targetController);
           countInputField.text = "";
           countInputField.interactable = true;
           ShowResultPanel(false);*/

        // Update UI
        UpdateTargetDisplay();
        //countInputField.text = "";
        //countInputField.interactable = true;
        ShowResultPanel(false);
        SetAnswerButtonsActive(false);

        // Start spawning
        ballSpawner.totalBallsToSpawn = ballsPerRound;
        ballSpawner.spawnDuration = roundDuration;
        ballSpawner.targetBallsToSpawn = targetBallsToSpawn; // ⭐ TELL SPAWNER HOW MANY TARGETS
        ballSpawner.StartSpawning(targetBallPrefab);

        Debug.Log($"New round started. Target: {targetBallPrefab.name}");
    }

    void Update()
    {
        if (gameActive)
        {
            gameTimer += Time.deltaTime;
            UpdateTimerDisplay();

            // Auto-end when time's up
            if (gameTimer >= roundDuration && !ballSpawner.isSpawning)
            {
                OnSpawningComplete();
            }
            else if (gameTimer >= roundDuration)
            {
                // Time's up but spawning isn't finished - wait for spawning to complete
                Debug.Log("Time's up, waiting for spawning to finish...");
            }
        }
    }

    void DisplayTargetBall()
    {
        //GameObject newBall1 = Instantiate(ballSpawner.ballPrefabs[0], targetDisplaySpawnPoint.position, Quaternion.identity);

        Debug.Log("in display atergter");
        if (currentTargetBallPrefab == null)
        {
            Debug.Log("target ball prefab na");
            currentTargetBallPrefab = targetBallPrefab;
            Debug.Log("target ball prefab na try1"+ currentTargetBallPrefab);
            
           
        }
        else if ( targetDisplaySpawnPoint != null )
        {
            Debug.Log("point na");

        }
        if (currentTargetBallPrefab != null && targetDisplaySpawnPoint != null)
        {
            displayedTargetBall = Instantiate(currentTargetBallPrefab, targetDisplaySpawnPoint.position, Quaternion.identity);
            //displayedTargetBall.GetComponent<BallController>().fallSpeed = 0;
            BallController ballController1 = displayedTargetBall.GetComponent<BallController>();
            ballController1.enabled = false;
            displayedTargetBall.transform.localScale = targetBallScale;
            Debug.Log("in display if");
            //GameObject newBall = Instantiate(currentTargetBallPrefab, targetDisplaySpawnPoint.position, Quaternion.identity);
            /*            // Disable movement for display ball
                        BallController controller = displayedTargetBall.GetComponent<BallController>();
                        if (controller != null)
                            controller.enabled = false;

                        // Make it static
                        Rigidbody rb = displayedTargetBall.GetComponent<Rigidbody>();
                        if (rb != null)
                            rb.isKinematic = true;*/
        }
    }

    void OnTargetBallSpawned(int currentCount)
    {
        actualTargetCount = currentCount;
    }

    void OnSpawningComplete()
    {
        gameActive = false;
        waitingForAnswer = true;
        //countInputField.interactable = false;
        Debug.Log($"Round complete. Waiting for player input. Actual count: {actualTargetCount}");
        GenerateAnswerOptions();
        SetAnswerButtonsActive(true);
    }
    void GenerateAnswerOptions()
    {
        // Create three possible answers: one correct, two wrong
        List<int> possibleAnswers = new List<int>();

        // Add correct answer
        possibleAnswers.Add(actualTargetCount);

        // Add wrong answers (ensure they're different from correct answer)
        int wrongAnswer1 = GetWrongAnswer(actualTargetCount);
        int wrongAnswer2 = GetWrongAnswer(actualTargetCount, wrongAnswer1);

        possibleAnswers.Add(wrongAnswer1);
        possibleAnswers.Add(wrongAnswer2);

        // Shuffle the answers
        ShuffleList(possibleAnswers);

        // Find which index has the correct answer
        correctAnswerIndex = possibleAnswers.IndexOf(actualTargetCount);

        // Update button texts
        button1Text.text = possibleAnswers[0].ToString();
        button2Text.text = possibleAnswers[1].ToString();
        button3Text.text = possibleAnswers[2].ToString();

        Debug.Log($"Correct answer is at index: {correctAnswerIndex} (Value: {actualTargetCount})");
    }

    int GetWrongAnswer(int correctAnswer, int otherWrongAnswer = -1)
    {
        int wrongAnswer;
        do
        {
            // Generate wrong answer within reasonable range
            int offset = Random.Range(1, 4); // Difference of 1-3
            if (Random.Range(0, 2) == 0) // Randomly add or subtract
            {
                wrongAnswer = correctAnswer + offset;
            }
            else
            {
                wrongAnswer = Mathf.Max(0, correctAnswer - offset); // Don't go below 0
            }
        }
        while (wrongAnswer == correctAnswer || wrongAnswer == otherWrongAnswer || wrongAnswer < 0);

        return wrongAnswer;
    }
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    void OnAnswerSelected(int buttonIndex)
    {
        if (!waitingForAnswer) return;

        waitingForAnswer = false;
        SetAnswerButtonsActive(false);

        bool isCorrect = (buttonIndex == correctAnswerIndex);
        EvaluateResults(isCorrect);
    }


    /*    public void SubmitCount()
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
        }*/

    /*    void EvaluateResults()
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
        }*/

    /*
        void EvaluateResults(bool isCorrect)
        {
            ShowResultPanel(true);

            //BallController targetController = currentTargetBallPrefab.GetComponent<BallController>();

            string resultText = $"<size=24><b>Round Complete!</b></size>\n\n";
            resultText += $"<b>Target:</b> {targetController.ballColor} {targetController.ballPattern}\n";
            resultText += $"<b>Actual Count:</b> {actualTargetCount}\n\n";

            if (isCorrect)
            {
                resultText += $"<color=green>✅ PERFECT! You got it right!</color>\n";
                resultText += $"<size=20>Excellent attention and counting skills!</size>";
            }
            else
            {
                resultText += $"<color=red>❌ Not quite right!</color>\n";
                resultText += $"<size=20>Keep practicing your observation skills!</size>";
            }

            resultDisplay.text = resultText;
        }*/

    void EvaluateResults(bool isCorrect)
    {
        ShowResultPanel(true);

        string resultText = $"<size=24><b>Round Complete!</b></size>\n\n";
        resultText += $"<b>Target Balls to Count:</b> {targetBallsToSpawn}\n";
        resultText += $"<b>Your Answer:</b> {GetSelectedAnswerText()}\n";
        resultText += $"<b>Correct Answer:</b> {targetBallsToSpawn}\n\n";

        if (isCorrect)
        {
            resultText += $"<color=green>✅ PERFECT! You got it right!</color>\n";
            resultText += $"<size=20>Excellent attention and counting skills!</size>";
        }
        else
        {
            resultText += $"<color=red>❌ Not quite right!</color>\n";
            resultText += $"<size=20>Keep practicing your observation skills!</size>";
        }

        resultDisplay.text = resultText;
    }

    // Helper method to get what the player selected
    string GetSelectedAnswerText()
    {
        // This would need to track which button was actually pressed
        // You'll need to store the selected answer when buttons are clicked
        return "Unknown"; // Placeholder - implement based on your button tracking
    }

    void SetAnswerButtonsActive(bool active)
    {
        answerButton1.gameObject.SetActive(active);
        answerButton2.gameObject.SetActive(active);
        answerButton3.gameObject.SetActive(active);

        if (active)
        {
            Debug.Log("Answer buttons activated");
        }
    }

    void UpdateTargetDisplay()
    {
/*        if (targetDisplay != null)
        {
            targetDisplay.text = $"Count how many:\n<size=36><b>{target.ballColor} {target.ballPattern}</b></size>\nballs appear below";
        }*/
        if (targetDisplay != null && targetBallPrefab != null)
        {
            BallController targetController = targetBallPrefab.GetComponent<BallController>();
            targetDisplay.text = $"Count how many:\n<size=36><b>{targetController.ballColor} {targetController.ballPattern}</b></size>\nballs appear below";
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