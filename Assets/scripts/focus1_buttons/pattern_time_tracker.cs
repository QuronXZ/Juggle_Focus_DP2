using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;


public class pattern_time_tracker : MonoBehaviour

{
    [System.Serializable]
    public class TrialData
    {
        public int trialNumber;
        public string expectedPattern;
        public string chosenPattern;
        public bool isCorrect;
        public float reactionTime; // Time from trial start to answer selection
        public float totalTrialTime; // From trial start to completion
        public string ballColor;
        public string ballMaterial;
        public int totalBallsInScene;
    }

    [System.Serializable]
    public class SessionData
    {
        public string sessionId;
        public string sceneName;
        public string startTime;
        public string endTime;
        public float totalSessionTime;
        public int totalTrials;
        public int correctAnswers;
        public int wrongAnswers;
        public float accuracyPercentage;
        public float averageReactionTime;
        public List<TrialData> trials = new List<TrialData>();
    }

    [Header("Tracking Settings")]
    public bool enableTracking = true;
    public string fileName = "PatternQuiz_Data";

    private SessionData currentSession;
    private TrialData currentTrial;
    private float sessionStartTime;
    private float trialStartTime;
    private int currentTrialNumber = 0;
    private bool isTrialActive = false;

    // References
    private pattern_quiz_mgr quizManager;
    private pattern_move_mgr patternManager;

    void Start()
    {
        quizManager = FindObjectOfType<pattern_quiz_mgr>();
        patternManager = FindObjectOfType<pattern_move_mgr>();

        if (enableTracking)
        {
            InitializeSession();
            SubscribeToEvents();
        }
    }

    void InitializeSession()
    {
        currentSession = new SessionData
        {
            sessionId = System.Guid.NewGuid().ToString(),
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            startTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            trials = new List<TrialData>()
        };

        sessionStartTime = Time.time;
        Debug.Log($"Session started: {currentSession.sessionId}");
    }

    void SubscribeToEvents()
    {
        // You'll need to modify pattern_quiz_mgr to expose events
        // For now, we'll use method calls from pattern_quiz_mgr
    }

    // ⭐ CALL THIS when a new trial starts
    public void OnTrialStarted(pattern_move_mgr.BallPattern ballPattern)
    {
        if (!enableTracking) return;

        currentTrialNumber++;
        trialStartTime = Time.time;
        isTrialActive = true;

        currentTrial = new TrialData
        {
            trialNumber = currentTrialNumber,
            expectedPattern = GetPatternName(ballPattern),
            ballColor = GetBallColor(ballPattern.ball),
            ballMaterial = GetBallMaterial(ballPattern.ball),
            totalBallsInScene = GetTotalBallsCount()
        };

        Debug.Log($"Trial {currentTrialNumber} started - Expected: {currentTrial.expectedPattern}");
    }

    // ⭐ CALL THIS when user selects an answer
    public void OnAnswerSelected(string chosenPattern, bool isCorrect)
    {
        if (!enableTracking || !isTrialActive) return;

        float reactionTime = Time.time - trialStartTime;

        currentTrial.chosenPattern = chosenPattern;
        currentTrial.isCorrect = isCorrect;
        currentTrial.reactionTime = reactionTime;
        currentTrial.totalTrialTime = reactionTime; // Same as reaction time for this implementation

        // Update session totals
        if (isCorrect)
            currentSession.correctAnswers++;
        else
            currentSession.wrongAnswers++;

        currentSession.trials.Add(currentTrial);
        isTrialActive = false;

        Debug.Log($"Trial {currentTrialNumber} completed - Correct: {isCorrect}, Reaction Time: {reactionTime:F2}s");
    }

    // ⭐ CALL THIS when the quiz ends
    public void OnQuizCompleted()
    {
        if (!enableTracking) return;

        currentSession.endTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        currentSession.totalSessionTime = Time.time - sessionStartTime;
        currentSession.totalTrials = currentSession.trials.Count;
        currentSession.accuracyPercentage = currentSession.totalTrials > 0 ?
            (float)currentSession.correctAnswers / currentSession.totalTrials * 100f : 0f;

        // Calculate average reaction time
        float totalReactionTime = 0f;
        foreach (var trial in currentSession.trials)
        {
            totalReactionTime += trial.reactionTime;
        }
        currentSession.averageReactionTime = currentSession.totalTrials > 0 ?
            totalReactionTime / currentSession.totalTrials : 0f;

        SaveSessionData();
        Debug.Log($"Quiz completed! Accuracy: {currentSession.accuracyPercentage:F1}%");
    }

    /*    void SaveSessionData()
        {
            string jsonData = JsonUtility.ToJson(currentSession, true);
            string filePath = GetFilePath();
            Debug.Log("WATERRR "+filePath);
            try
            {
                File.WriteAllText(filePath, jsonData, Encoding.UTF8);
                Debug.Log($"Session data saved to: {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save session data: {e.Message}");
            }
        }*/
    /*
        string GetFilePath()
        {
            string directory = Application.persistentDataPath + "/QuizData/";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory + fileName + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
        }
    */
    // Helper to make CSV-safe strings
    string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";

        // Escape internal quotes
        value = value.Replace("\"", "\"\"");

        // Wrap in quotes in case of commas, semicolons, etc.
        return "\"" + value + "\"";
    }

    void SaveSessionData()
    {
        string filePath = GetFilePath();
        Debug.Log("Saving Pattern Telemetry CSV to: " + filePath);

        try
        {
            var sb = new StringBuilder();

            // Header row
            sb.AppendLine("SessionId,SceneName,StartTime,EndTime,TotalSessionTime,TrialNumber,ExpectedPattern,ChosenPattern,IsCorrect,ReactionTime,TotalTrialTime,BallColor,BallMaterial,TotalBallsInScene");

            foreach (var trial in currentSession.trials)
            {
                string line = string.Format(
                    "{0},{1},{2},{3},{4:F3},{5},{6},{7},{8},{9:F3},{10:F3},{11},{12},{13}",
                    EscapeCsv(currentSession.sessionId),
                    EscapeCsv(currentSession.sceneName),
                    EscapeCsv(currentSession.startTime),
                    EscapeCsv(currentSession.endTime),
                    currentSession.totalSessionTime,
                    trial.trialNumber,
                    EscapeCsv(trial.expectedPattern),
                    EscapeCsv(trial.chosenPattern),
                    trial.isCorrect ? "TRUE" : "FALSE",
                    trial.reactionTime,
                    trial.totalTrialTime,
                    EscapeCsv(trial.ballColor),
                    EscapeCsv(trial.ballMaterial),
                    trial.totalBallsInScene
                );

                sb.AppendLine(line);
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            Debug.Log($"Session data saved to CSV: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save session CSV data: {e.Message}");
        }
    }

    string GetFilePath()
    {
        string directory = Application.persistentDataPath + "/QuizData/";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // File base name as requested: Pattern_telemetry_timer
        string fileBaseName = "Pattern_telemetry_timer";
        string timeStamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        return Path.Combine(directory, fileBaseName + "_" + timeStamp + ".csv");
    }
    // Utility methods
    string GetPatternName(pattern_move_mgr.BallPattern bp)
    {
        if (bp == null || bp.pattern == null) return "Unknown";
        return bp.pattern.patternName;
    }

    string GetBallColor(ball_script ball)
    {
        if (ball == null) return "Unknown";
        var renderer = ball.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            return renderer.material.color.ToString();
        }
        return "Unknown";
    }

    string GetBallMaterial(ball_script ball)
    {
        if (ball == null) return "Unknown";
        var renderer = ball.GetComponent<MeshRenderer>();
        return renderer != null && renderer.material != null ? renderer.material.name : "Unknown";
    }

    int GetTotalBallsCount()
    {
        return patternManager != null && patternManager.ballPatterns != null ?
            patternManager.ballPatterns.Count : 0;
    }

    // Public access to current data
    public SessionData GetCurrentSessionData() => currentSession;
    public TrialData GetCurrentTrialData() => currentTrial;
    public float GetCurrentTrialTime() => isTrialActive ? Time.time - trialStartTime : 0f;
}