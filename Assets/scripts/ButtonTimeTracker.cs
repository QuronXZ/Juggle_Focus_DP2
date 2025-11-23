using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonTimeTracker : MonoBehaviour
{
    [System.Serializable]
    public class DrawingAttempt
    {
        public int attemptNumber;
        public float startTime;
        public float endTime;
        public float duration;
        public bool wasSuccessful;

        public DrawingAttempt(int number, float start)
        {
            attemptNumber = number;
            startTime = start;
            endTime = 0f;
            duration = 0f;
            wasSuccessful = false;
        }
    }

    [System.Serializable]
    public class ButtonTimeData
    {
        public string buttonName;
        public float startTime;
        public float totalElapsedTime;
        public bool isCurrentlyActive;
        public int usageCount;
        public List<DrawingAttempt> drawingAttempts;
        public int currentAttemptNumber;

        public ButtonTimeData(string name)
        {
            buttonName = name;
            startTime = 0f;
            totalElapsedTime = 0f;
            isCurrentlyActive = false;
            usageCount = 0;
            drawingAttempts = new List<DrawingAttempt>();
            currentAttemptNumber = 0;
        }
    }

    [Header("UI References")]
    public TextMeshProUGUI timerDisplay;
    public TextMeshProUGUI resultsDisplay;
    public TextMeshProUGUI attemptsDisplay; // New UI for attempts

    [Header("Tracking Settings")]
    public bool isTracking = false;

    // Dictionary to store time data for each button
    private Dictionary<int, ButtonTimeData> timeData = new Dictionary<int, ButtonTimeData>();
    private int currentActiveButton = -1;
    private float sessionStartTime;

    // References
    private UIManager_DrawVerify uiManager;
    private line_renderer drawingManager;

    void Start()
    {
        // Get reference to your existing UI manager
        uiManager = FindObjectOfType<UIManager_DrawVerify>();
        drawingManager = FindObjectOfType<line_renderer>();

        // Initialize time data for all buttons
        InitializeTimeData();

        // Subscribe to button events
        SubscribeToButtonEvents();

        Debug.Log("ButtonTimeTracker initialized");
    }

    void InitializeTimeData()
    {
        timeData.Clear();

        // Create time data entries for each button
        timeData.Add(uiManager.redIndex, new ButtonTimeData("Red"));
        timeData.Add(uiManager.yellowIndex, new ButtonTimeData("Yellow"));
        timeData.Add(uiManager.blueIndex, new ButtonTimeData("Blue"));

        Debug.Log($"Initialized time data for {timeData.Count} buttons");
    }

    void SubscribeToButtonEvents()
    {
        // Subscribe to your existing button events
        uiManager.redButton.onClick.AddListener(() => OnButtonPressed(uiManager.redIndex));
        uiManager.yellowButton.onClick.AddListener(() => OnButtonPressed(uiManager.yellowIndex));
        uiManager.blueButton.onClick.AddListener(() => OnButtonPressed(uiManager.blueIndex));

        Debug.Log("Subscribed to button events");
    }

    void Update()
    {
        if (isTracking && currentActiveButton != -1)
        {
            UpdateTimerDisplay();
            UpdateAttemptsDisplay();
        }
    }

    // Called when drawing starts (hook this up to your line_renderer StartDrawing method)
    public void OnDrawingStarted()
    {
        if (currentActiveButton != -1 && timeData.ContainsKey(currentActiveButton))
        {
            var buttonData = timeData[currentActiveButton];
            buttonData.currentAttemptNumber++;

            // Create new attempt
            DrawingAttempt newAttempt = new DrawingAttempt(
                buttonData.currentAttemptNumber,
                Time.time
            );

            buttonData.drawingAttempts.Add(newAttempt);

            Debug.Log($"Started drawing attempt #{buttonData.currentAttemptNumber} for {buttonData.buttonName} at time: {newAttempt.startTime}");
        }
    }

    // Called when drawing ends (hook this up to your line_renderer StopDrawing method)
    public void OnDrawingEnded(bool wasSuccessful = false)
    {
        if (currentActiveButton != -1 && timeData.ContainsKey(currentActiveButton))
        {
            var buttonData = timeData[currentActiveButton];

            if (buttonData.drawingAttempts.Count > 0)
            {
                DrawingAttempt currentAttempt = buttonData.drawingAttempts[buttonData.drawingAttempts.Count - 1];
                currentAttempt.endTime = Time.time;
                currentAttempt.duration = currentAttempt.endTime - currentAttempt.startTime;
                currentAttempt.wasSuccessful = wasSuccessful;

                Debug.Log($"Ended drawing attempt #{currentAttempt.attemptNumber} for {buttonData.buttonName}. " +
                         $"Duration: {currentAttempt.duration:F2}s, Successful: {wasSuccessful}");

                // Update total time for this button
                buttonData.totalElapsedTime += currentAttempt.duration;
            }
        }
    }

    public void OnButtonPressed(int buttonIndex)
    {
        Debug.Log($"Button pressed: {buttonIndex}, Current active: {currentActiveButton}");

        if (!isTracking)
        {
            // Start tracking session when first button is pressed
            StartTrackingSession();
        }

        // If clicking the same button again, do nothing (already active)
        if (currentActiveButton == buttonIndex)
        {
            Debug.Log("Same button clicked again, ignoring");
            return;
        }

        // End timer for previous button (if any)
        if (currentActiveButton != -1 && timeData.ContainsKey(currentActiveButton))
        {
            EndButtonTimer(currentActiveButton);
        }

        // Start timer for new button
        StartButtonTimer(buttonIndex);

        // Update current active button
        currentActiveButton = buttonIndex;

        Debug.Log($"Now tracking: {timeData[buttonIndex].buttonName}");
    }

    void StartTrackingSession()
    {
        isTracking = true;
        sessionStartTime = Time.time;
        Debug.Log("Time tracking session started at: " + sessionStartTime);
    }

    void StartButtonTimer(int buttonIndex)
    {
        if (timeData.ContainsKey(buttonIndex))
        {
            var buttonData = timeData[buttonIndex];
            buttonData.startTime = Time.time;
            buttonData.isCurrentlyActive = true;
            buttonData.usageCount++;

            Debug.Log($"Started timer for {buttonData.buttonName} button at time: {buttonData.startTime}");
            Debug.Log($"This is usage # {buttonData.usageCount} for {buttonData.buttonName}");
        }
    }

    void EndButtonTimer(int buttonIndex)
    {
        if (timeData.ContainsKey(buttonIndex) && timeData[buttonIndex].isCurrentlyActive)
        {
            var buttonData = timeData[buttonIndex];
            float endTime = Time.time;
            float thisSessionTime = endTime - buttonData.startTime;
            buttonData.totalElapsedTime += thisSessionTime;
            buttonData.isCurrentlyActive = false;

            Debug.Log($"Ended timer for {buttonData.buttonName} button");
            Debug.Log($"This session: {thisSessionTime:F2}s, Total: {buttonData.totalElapsedTime:F2}s");
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerDisplay != null && timeData.ContainsKey(currentActiveButton))
        {
            var buttonData = timeData[currentActiveButton];
            float currentTime = Time.time - buttonData.startTime;
            timerDisplay.text = $"{buttonData.buttonName}: {currentTime:F1}s (Total: {buttonData.totalElapsedTime:F1}s)";
        }
    }

    void UpdateAttemptsDisplay()
    {
        if (attemptsDisplay != null && timeData.ContainsKey(currentActiveButton))
        {
            var buttonData = timeData[currentActiveButton];
            string attemptsText = $"{buttonData.buttonName} Attempts: {buttonData.drawingAttempts.Count}";

            if (buttonData.drawingAttempts.Count > 0)
            {
                var lastAttempt = buttonData.drawingAttempts[buttonData.drawingAttempts.Count - 1];
                attemptsText += $"\nCurrent: #{lastAttempt.attemptNumber}";

                if (lastAttempt.endTime > 0)
                {
                    attemptsText += $", Last: {lastAttempt.duration:F1}s";
                }
            }

            attemptsDisplay.text = attemptsText;
        }
    }

    // Call this when the game ends or when you want to finalize all timers
    public void EndAllTimers()
    {
        Debug.Log("Ending all timers...");

        // End timer for current active button
        if (currentActiveButton != -1)
        {
            EndButtonTimer(currentActiveButton);
        }

        isTracking = false;

        // Display results
        DisplayResults();
    }

    void DisplayResults()
    {
        string results = "=== FINAL RESULTS ===\n\n";
        bool hasData = false;

        foreach (var data in timeData.Values)
        {
            results += $"{data.buttonName}:\n";
            results += $"  Total Time: {data.totalElapsedTime:F2}s\n";
            results += $"  Usage Count: {data.usageCount}\n";
            results += $"  Drawing Attempts: {data.drawingAttempts.Count}\n";

            if (data.drawingAttempts.Count > 0)
            {
                hasData = true;
                results += $"  Attempt Details:\n";

                foreach (var attempt in data.drawingAttempts)
                {
                    string status = attempt.wasSuccessful ? "✓" : "✗";
                    results += $"    Attempt #{attempt.attemptNumber}: {attempt.duration:F2}s {status}\n";
                }

                // Calculate averages
                float totalAttemptTime = 0f;
                int successfulAttempts = 0;
                foreach (var attempt in data.drawingAttempts)
                {
                    totalAttemptTime += attempt.duration;
                    if (attempt.wasSuccessful) successfulAttempts++;
                }

                float avgTime = totalAttemptTime / data.drawingAttempts.Count;
                float successRate = (float)successfulAttempts / data.drawingAttempts.Count * 100f;

                results += $"  Average Time: {avgTime:F2}s\n";
                results += $"  Success Rate: {successRate:F1}%\n";
            }
            else
            {
                results += $"  No drawing attempts recorded\n";
            }
            results += "\n";
        }

        if (!hasData)
        {
            results += "No drawing attempt data recorded!\n";
        }

        // Calculate total time
        float totalTime = Time.time - sessionStartTime;
        results += $"\nTotal Session Time: {totalTime:F2}s";

        Debug.Log("=== FINAL TIME RESULTS ===");
        Debug.Log(results);

        // Display in UI if available
        if (resultsDisplay != null)
        {
            resultsDisplay.text = results;
        }
    }

    // Public methods to access the data
    public Dictionary<int, ButtonTimeData> GetTimeData()
    {
        return new Dictionary<int, ButtonTimeData>(timeData);
    }

    public float GetButtonTime(int buttonIndex)
    {
        if (timeData.ContainsKey(buttonIndex))
        {
            return timeData[buttonIndex].totalElapsedTime;
        }
        return 0f;
    }

    public List<DrawingAttempt> GetButtonAttempts(int buttonIndex)
    {
        if (timeData.ContainsKey(buttonIndex))
        {
            return timeData[buttonIndex].drawingAttempts;
        }
        return new List<DrawingAttempt>();
    }

    public void ResetAllTimers()
    {
        timeData.Clear();
        InitializeTimeData();
        currentActiveButton = -1;
        isTracking = false;

        if (timerDisplay != null)
            timerDisplay.text = "00.0s";

        if (attemptsDisplay != null)
            attemptsDisplay.text = "Attempts: 0";

        Debug.Log("All timers reset");
    }

    // Debug method to check current state
    public void DebugCurrentState()
    {
        Debug.Log("=== Current Time Tracker State ===");
        Debug.Log($"Is Tracking: {isTracking}");
        Debug.Log($"Current Active Button: {currentActiveButton}");
        Debug.Log($"Session Start Time: {sessionStartTime}");

        foreach (var kvp in timeData)
        {
            var data = kvp.Value;
            Debug.Log($"{data.buttonName}: Active={data.isCurrentlyActive}, StartTime={data.startTime}, TotalTime={data.totalElapsedTime:F2}s, UsageCount={data.usageCount}, Attempts={data.drawingAttempts.Count}");

            foreach (var attempt in data.drawingAttempts)
            {
                Debug.Log($"  Attempt #{attempt.attemptNumber}: {attempt.duration:F2}s, Successful={attempt.wasSuccessful}");
            }
        }
    }
}