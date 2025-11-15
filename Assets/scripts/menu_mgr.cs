using UnityEngine;
using UnityEngine.SceneManagement;

public class menu_mgr : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject help_Panel;

    private void Start()
    {
        help_Panel.SetActive(false);
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("focus1"); // Replace with your actual scene name
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
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
