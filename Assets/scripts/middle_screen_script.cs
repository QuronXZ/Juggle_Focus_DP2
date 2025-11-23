using UnityEngine;
using UnityEngine.SceneManagement;

public class middle_screen_script: MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //public GameObject help_Panel;

    private void Start()
    {
        //help_Panel.SetActive(false);
    }
    public void button1()
    {
        SceneManager.LoadScene("focus1_phone_lvl1"); // Replace with your actual scene name
    }
    public void button2()
    {
        SceneManager.LoadScene("ball_selector"); // Replace with your actual scene name
    }
    public void Open_help()
    {
        //help_Panel.SetActive(true);
        //transform.DOScale(help_Panel.transform.localScale * 1.1f, 0.2f).SetEase(Ease.OutBack);
    }
    public void go_menu()
    {
        SceneManager.LoadScene("Main_Menu"); // Replace with your actual scene name
    }
    public void Close_help()
    {
        //help_Panel.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
