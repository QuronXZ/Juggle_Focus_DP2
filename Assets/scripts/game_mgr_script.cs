using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class game_mgr_script : MonoBehaviour
{
    [Header("System References")]
    public Pattern_movement_script patternSystem;

    [Header("Ball References")]
    public List<GameObject> ballObjects = new List<GameObject>();

    [Header("UI References")]
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;
    public Button[] patternButtons;

    [Header("Game Settings")]
    public int roundsPerGame = 10;
    public float roundDelay = 1.5f;

    private GameObject currentTargetBall;
    private int currentRound = 0;
    private int score = 0;
    private bool waitingForInput = true;

    // Pattern names for display
    private readonly string[] patternNames = {
        "CIRCLE", "INFINITY", "WAVE", "TRIANGLE",
        "SQUARE", "FIGURE-8", "LISSAJOUS", "SPIRAL"
    };

    void Start()
    {
        InitializeSystems();
        SetupUI();
        StartCoroutine(GameLoop());
    }

    void InitializeSystems()
    {
        // Find pattern system if not assigned
        if (patternSystem == null)
            patternSystem = FindFirstObjectByType<Pattern_movement_script>();

        // Initialize balls with random patterns
        InitializeBalls();
    }

    void InitializeBalls()
    {
        foreach (GameObject ball in ballObjects)
        {
            if (ball != null)
            {
                ball_appearance_script appearance = ball.GetComponent<ball_appearance_script>();
                Pattern_movement_script.PatternType randomPattern = GetRandomPattern();

                // Register with pattern system
                patternSystem.AddBall(ball, randomPattern, Random.Range(0.8f, 1.5f));

                // Set ball appearance name based on color
                if (appearance != null)
                {
                    appearance.ballName = GetColorName(appearance.ballColor);
                }
            }
        }

        // Debug: Print initial patterns
        patternSystem.PrintBallPatterns();
    }

    void SetupUI()
    {
        // Setup pattern buttons
        for (int i = 0; i < patternButtons.Length && i < 8; i++)
        {
            int patternIndex = i; // Important: capture value for closure
            patternButtons[i].onClick.RemoveAllListeners();
            patternButtons[i].onClick.AddListener(() => OnPatternSelected(patternIndex));

            // Update button text
            Text buttonText = patternButtons[i].GetComponentInChildren<Text>();
            if (buttonText != null && i < patternNames.Length)
            {
                buttonText.text = patternNames[i];
            }
        }

        UpdateScoreDisplay();
    }

    IEnumerator GameLoop()
    {
        while (currentRound < roundsPerGame)
        {
            yield return StartCoroutine(PlayRound());
            currentRound++;
            yield return new WaitForSeconds(roundDelay);
        }

        // Game completed
        ShowFeedback("Game Complete! Final Score: " + score, Color.yellow);
    }

    IEnumerator PlayRound()
    {
        // Ensure we have valid balls
        if (ballObjects == null || ballObjects.Count == 0)
        {
            Debug.LogError("No ball objects assigned!");
            yield break;
        }

        // Choose target ball from available balls
        currentTargetBall = ballObjects[Random.Range(0, ballObjects.Count)];

        // Safety check
        if (currentTargetBall == null)
        {
            Debug.LogError("Selected target ball is null!");
            yield break;
        }

        // Highlight target ball
        SetBallHighlights(currentTargetBall);

        // Update UI
        UpdateTargetText();
        ShowFeedback("Watch the highlighted ball!", Color.cyan);

        // Wait a moment for player to see which ball is target
        yield return new WaitForSeconds(1f);

        // Remove highlight but keep balls moving
        SetBallHighlights(null);

        // Wait for player input
        waitingForInput = true;
        ShowFeedback("What pattern did you see?", Color.white);

        float timeout = 10f; // 10 second timeout
        float timer = 0f;

        while (waitingForInput && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (timer >= timeout)
        {
            ShowFeedback("Time's up! Moving to next round.", Color.gray);
        }
    }


    void OnPatternSelected(int patternIndex)
    {
        if (!waitingForInput || currentTargetBall == null) return;

        waitingForInput = false;

        Pattern_movement_script.PatternType selectedPattern = (Pattern_movement_script.PatternType)patternIndex;
        Pattern_movement_script.PatternType actualPattern = patternSystem.GetBallPattern(currentTargetBall);

        bool isCorrect = (selectedPattern == actualPattern);

        if (isCorrect)
        {
            score++;
            ShowFeedback("✓ Correct! " + patternNames[patternIndex] + " pattern!", Color.green);
        }
        else
        {
            ShowFeedback("✗ Wrong! It was " + patternNames[(int)actualPattern], Color.red);
        }

        UpdateScoreDisplay();
    }

    void SetBallHighlights(GameObject targetBall)
    {
        foreach (GameObject ball in ballObjects)
        {
            ball_appearance_script appearance = ball.GetComponent<ball_appearance_script>();
            if (appearance != null)
            {
                appearance.SetHighlight(ball == targetBall);
            }
        }
    }

    void UpdateTargetText()
    {
        if (targetText != null && currentTargetBall != null)
        {
            ball_appearance_script appearance = currentTargetBall.GetComponent<ball_appearance_script>();
            targetText.text = "Track: " + appearance.ballName + " Ball";
        }
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score + "/" + roundsPerGame +
                           " | Round: " + (currentRound + 1) + "/" + roundsPerGame;
        }
    }

    void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
        Debug.Log("Game: " + message);
    }

    Pattern_movement_script.PatternType GetRandomPattern()
    {
        // Weight towards simpler patterns for better gameplay
        float rand = Random.value;
        if (rand < 0.5f) return Pattern_movement_script.PatternType.Circle;
        if (rand < 0.7f) return Pattern_movement_script.PatternType.Infinity;
        if (rand < 0.85f) return Pattern_movement_script.PatternType.Wave;
        if (rand < 0.9f) return Pattern_movement_script.PatternType.Triangle;
        if (rand < 0.95f) return Pattern_movement_script.PatternType.Figure8;
        return (Pattern_movement_script.PatternType)Random.Range(0, 8);
    }

    string GetColorName(Color color)
    {
        if (color == Color.red) return "Red";
        if (color == Color.blue) return "Blue";
        if (color == Color.green) return "Green";
        if (color == Color.yellow) return "Yellow";
        if (color == Color.magenta) return "Purple";
        if (color == Color.cyan) return "Cyan";
        return "Colorful";
    }

    // Public method for UI to restart game
    public void RestartGame()
    {
        currentRound = 0;
        score = 0;
        StopAllCoroutines();
        StartCoroutine(GameLoop());
    }
}