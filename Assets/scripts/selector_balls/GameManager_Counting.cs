using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;

public class GameManager_Counting : MonoBehaviour
{
    [Header("References")]
    public BallSpawner ballSpawner;
    public TextMeshProUGUI targetDisplay;
    public TextMeshProUGUI targetDisplay2;
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
    public Transform targetDisplaySpawnPoint2; // Where to show the target ball

    public Vector3 targetBallScale = new Vector3(2f, 2f, 2f); // Larger for visibility

    [Header("Game Settings")]
    public float start_delay_time = 5f;
    public GameObject targetBallPrefab; // ⭐
    public int ballsPerRound = 15;
    public float roundDuration = 10f;
    public int targetBallsToSpawn = 6; // ⭐
                                       // At top of GameManager_Counting:
    public uiSliderScript uiSliderScript;


    private int playerSelectedAnswer = -1;
    private GameObject currentTargetBallPrefab;
    private GameObject displayedTargetBall;
    private GameObject displayedTargetBall2;
    private int actualTargetCount = 0;
    private int correctAnswerIndex = 0;
    //private int playerCount = 0;
    private float gameTimer = 0f;
    private bool gameActive = false;
    private bool waitingForAnswer = false;
    private float answerStartTime = -1f;
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

        timerDisplay.enabled = false;
        targetDisplay.enabled = false; 
        targetDisplay2.enabled = true;
        //gamePanel.SetActive(false);

        //ShowResultPanel(false);
        //resultPanel.SetActive(false);
        /*//StartCoroutine(StartRoundWithDelay()); 
        StartNewRound();*/
        // Show INTRO target ball at spawn point 2
        DisplayIntroTargetBall();
        UpdateTargetDisplay();

        // Start the 5-second intro delay, then start the actual round
        StartCoroutine(StartRoundWithDelay());
    }

    void DisplayIntroTargetBall()
    {
        if (targetBallPrefab != null && targetDisplaySpawnPoint2 != null)
        {
            displayedTargetBall2 = Instantiate(targetBallPrefab, targetDisplaySpawnPoint2.position, Quaternion.identity);

            BallController bc = displayedTargetBall2.GetComponent<BallController>();
            if (bc != null) bc.enabled = false;

            displayedTargetBall2.transform.localScale = targetBallScale;
        }
    }
    /*    public void StartNewRound()
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

            *//*        // Choose random target from available prefabs
                    currentTargetBallPrefab = ballSpawner.ballPrefabs[Random.Range(0, ballSpawner.ballPrefabs.Count)];
                    BallController targetController = currentTargetBallPrefab.GetComponent<BallController>();
                    string targetID = targetController.ballID;
            *//*
            // Display the target ball
            DisplayTargetBall();

            *//*   // Update UI
               UpdateTargetDisplay(targetController);
               countInputField.text = "";
               countInputField.interactable = true;
               ShowResultPanel(false);*//*

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

            StartCoroutine(StartRoundWithDelay());


            Debug.Log($"New round started. Target: {targetBallPrefab.name}");
        }*/

    public void StartNewRound()
    {
        // Clean up previous main target display
        if (displayedTargetBall != null)
            Destroy(displayedTargetBall);

        // Reset game state
        actualTargetCount = 0;
        gameTimer = 0f;
        gameActive = true;
        waitingForAnswer = false;

        // Display the MAIN target ball and text (now visible because intro is over)
        DisplayTargetBall();
        UpdateTargetDisplay();

        ShowResultPanel(false);
        SetAnswerButtonsActive(false);

        // Configure spawner
        ballSpawner.totalBallsToSpawn = ballsPerRound;
        ballSpawner.spawnDuration = roundDuration;
        ballSpawner.targetBallsToSpawn = targetBallsToSpawn;

        // Start spawning balls immediately (we already waited 5s before calling this)
        ballSpawner.StartSpawning(targetBallPrefab);

        Debug.Log($"New round started. Target: {targetBallPrefab.name}");
    }


    /*
        IEnumerator StartRoundWithDelay()
        {
            yield return new WaitForSeconds(start_delay_time);  // 5 second delay before the game starts
                                                                //StartNewRound();


            targetDisplay2.enabled = false;

            timerDisplay.enabled= true;
            targetDisplay.enabled=true;
            gamePanel.SetActive(false);
            //Destroy(displayedTargetBall2);
            //displayedTargetBall2.SetActive(false);
            ballSpawner.StartSpawning(targetBallPrefab);
            StartNewRound();
        }*/
    IEnumerator StartRoundWithDelay()
    {
        // INTRO PHASE: targetDisplay2 + displayedTargetBall2 are visible now
        yield return new WaitForSeconds(start_delay_time);  // wait 5 seconds

        // Hide INTRO UI
        if (targetDisplay2 != null)
            targetDisplay2.enabled = false;

        if (displayedTargetBall2 != null)
            Destroy(displayedTargetBall2);

        // Show GAME UI
        if (timerDisplay != null)
            timerDisplay.enabled = true;

        if (targetDisplay != null)
            targetDisplay.enabled = true;

        gamePanel.SetActive(true);

        // Now actually start the round: create main target ball + spawn falling balls
        StartNewRound();
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
        Debug.Log("Water: "+actualTargetCount);
    }

    void OnSpawningComplete()
    {
        gameActive = false;
        waitingForAnswer = true;
        //countInputField.interactable = false;
        Debug.Log($"Round complete. Waiting for player input. Actual count: {actualTargetCount}");
        GenerateAnswerOptions();

        uiSliderScript.PlayEndSprite();
        SetAnswerButtonsActive(true);
        answerStartTime = Time.time;
    }
    /*void GenerateAnswerOptions()
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
        //ShuffleList(possibleAnswers);

        // Find which index has the correct answer
        correctAnswerIndex = possibleAnswers.IndexOf(actualTargetCount);

        // Update button texts
        button1Text.text = possibleAnswers[0].ToString();
        button2Text.text = possibleAnswers[1].ToString();
        button3Text.text = possibleAnswers[2].ToString();

        Debug.Log($"Correct answer is at index: {correctAnswerIndex} (Value: {actualTargetCount})");
    }*/


    void GenerateAnswerOptions()
    {
        List<int> possibleAnswers = new List<int>();

        // Add correct answer
        possibleAnswers.Add(actualTargetCount);

        // Add wrong answers
        int wrong1 = GetWrongAnswer(actualTargetCount);
        int wrong2 = GetWrongAnswer(actualTargetCount, wrong1);

        possibleAnswers.Add(wrong1);
        possibleAnswers.Add(wrong2);

        // ⭐ SHUFFLE THE ANSWERS — this line fixes your issue
        ShuffleList(possibleAnswers);

        // Find index of correct answer AFTER shuffle
        correctAnswerIndex = possibleAnswers.IndexOf(actualTargetCount);

        // Assign to buttons
        button1Text.text = possibleAnswers[0].ToString();
        button2Text.text = possibleAnswers[1].ToString();
        button3Text.text = possibleAnswers[2].ToString();

        Debug.Log($"Correct answer now at button index: {correctAnswerIndex}");
    }


    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
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
    /*    void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }*/
    /*    void OnAnswerSelected(int buttonIndex)
        {
            if (!waitingForAnswer) return;

            waitingForAnswer = false;
            SetAnswerButtonsActive(false);

            bool isCorrect = (buttonIndex == correctAnswerIndex);
            EvaluateResults(isCorrect);
        }
    */
    void OnAnswerSelected(int buttonIndex)
    {
        if (!waitingForAnswer) return;

        // SAVE player's chosen answer
        if (buttonIndex == 0) playerSelectedAnswer = int.Parse(button1Text.text);
        if (buttonIndex == 1) playerSelectedAnswer = int.Parse(button2Text.text);
        if (buttonIndex == 2) playerSelectedAnswer = int.Parse(button3Text.text);

        waitingForAnswer = false;
        SetAnswerButtonsActive(false);

        bool isCorrect = (buttonIndex == correctAnswerIndex);

        // ⭐ STOP REACTION TIMER
        float answerTime = -1f;
        if (answerStartTime > 0f)
        {
            answerTime = Time.time - answerStartTime;
            Debug.Log($"Player answer time: {answerTime:F3} seconds");
        }

        // ⭐ LOG TO CSV (if logger exists)
        if (spawner_timer.Instance != null)
        {
            spawner_timer.Instance.LogRound(
                roundDuration: roundDuration,
                ballsPerRound: ballsPerRound,
                targetBallsToSpawn: targetBallsToSpawn,
                answerTime: answerTime,
                isCorrect: isCorrect
            );
        }
        else
        {
            Debug.LogWarning("CountingTimerLogger.Instance is null — did you add it to a scene?");
        }
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

        //string resultText = $"<size=24><b>Round Complete!</b></size>\n\n";
        //resultText += $"<b>Target Balls to Count:</b> {targetBallsToSpawn}\n";
        string resultText = $"<b>Your Answer:</b> {GetSelectedAnswerText()}\n";
        resultText += $"<b>Correct Answer:</b> {actualTargetCount}\n\n";

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
        //return "Unknown"; // Placeholder - implement based on your button tracking

        return playerSelectedAnswer >= 0 ? playerSelectedAnswer.ToString() : "None";
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
            targetDisplay.text = $"Look out for <size=36><b></b></size> this ball:";
            targetDisplay2.text = $"Count how many:<size=36><b>{targetController.ballColor} {targetController.ballPattern}</b></size>balls appear!";


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

/*    public void OnPlayAgainClicked()
    {
        StartNewRound();
    }*/



    public void ReplayLevel()
    {
        Time.timeScale = 1f;  // ensure game is unpaused
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        // Option A: Using scene name
        SceneManager.LoadScene("Main_Menu");

        // Option B: Using build index (if your main menu is scene 0)
        // SceneManager.LoadScene(0);
    }
    public void GoToMiddleMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("middle_screen"); // Replace with your main menu scene name
    }
    public void PlayNextLevel()
    {
        Time.timeScale = 1f;

        string current = SceneManager.GetActiveScene().name;

        if (current == "ball_selector_lvl1")
        {
            SceneManager.LoadScene("ball_selector_lvl2");
        }
        else if (current == "ball_selector_lvl2")
        {
            SceneManager.LoadScene("ball_selector_lvl3");
        }
        else if (current == "ball_selector_lvl3")
        {
            SceneManager.LoadScene("Main_Menu");
        }
        else
        {
            Debug.Log("Unknown scene, sending to Main Menu as fallback.");
            SceneManager.LoadScene("Main_Menu");
        }
    }
}