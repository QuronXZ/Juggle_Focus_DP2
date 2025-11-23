using UnityEngine;
using UnityEngine.SceneManagement;

public class pause_menu : MonoBehaviour
{
 

    public GameObject pauseMenuUI; // Assign your Pause Menu panel here
    //public GameManager gameManager;
    private bool isPaused = false;
    public GameObject help_Panel;

    private void Start()
    {
        pauseMenuUI.SetActive(false);
        help_Panel.SetActive(false);
    }
    void Update()
    {
        // Toggle pause when Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }


    }

    public void Resume()
    {
        help_Panel.SetActive(false);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Resume game time
        isPaused = false;

    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze game time
        isPaused = true;

    }
    public void Open_help()
    {
        help_Panel.SetActive(true);
        //transform.DOScale(help_Panel.transform.localScale * 1.1f, 0.2f).SetEase(Ease.OutBack);
    }
    public void Close_help()
    {
        help_Panel.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f; // Reset time scale before loading scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main_Menu"); // Replace with your main menu scene name
    }
    public void GoToMiddleMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("middle_screen"); // Replace with your main menu scene name
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game");
        Application.Quit();

        // For editor testing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

 
    public void help_press_trail()
    {
        Debug.Log("Turning on trial");
    }
    public void help_press_music()
    {
        Debug.Log("Turning on trial");
    }

}
