using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
public class spawner_timer : MonoBehaviour


{
    public static spawner_timer Instance;

    private string filePath;

    private void Awake()
    {
        // Simple singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // CSV file path (in persistent data folder)
        filePath = Path.Combine(Application.persistentDataPath, "CountingStats.csv");
        Debug.Log(filePath);
        // If file doesn't exist, write header row
        if (!File.Exists(filePath))
        {
            string header = "Timestamp,SceneName,RoundDuration,BallsPerRound,TargetBallsToSpawn,AnswerTime,IsCorrect\n";
            File.WriteAllText(filePath, header, Encoding.UTF8);
        }
    }

    /// <summary>
    /// Log one round's data to the CSV.
    /// </summary>
    public void LogRound(
        float roundDuration,
        int ballsPerRound,
        int targetBallsToSpawn,
        float answerTime,
        bool isCorrect
    )
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string timeStamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string line = string.Format(
            "{0},{1},{2},{3},{4},{5:F3},{6}\n",
            timeStamp,
            sceneName,
            roundDuration,
            ballsPerRound,
            targetBallsToSpawn,
            answerTime,
            isCorrect ? "TRUE" : "FALSE"
        );
        WriteLineToFile(line);

        //File.AppendAllText(filePath, line, Encoding.UTF8);

        //Debug.Log("[CountingTimerLogger] Logged: " + line);
    }

    private void WriteLineToFile(string line)
    {
        try
        {
            // Use FileStream + StreamWriter so we can control sharing
            using (var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var sw = new StreamWriter(fs, Encoding.UTF8))
            {
                sw.WriteLine(line);
            }

            Debug.Log("[spawner_timer] Logged: " + line);
        }
        catch (IOException ex)
        {
            Debug.LogWarning("[spawner_timer] Failed to log (file may be in use): " + ex.Message);
        }
    }
    
}
