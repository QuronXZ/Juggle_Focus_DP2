//using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class main_menu_mgr : MonoBehaviour
{
    public GameObject help_Panel;
    public void PlayGame()
    {
        SceneManager.LoadScene("Cutscene"); // Replace with your actual scene name
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
// Assign these to button OnClick events in the inspector
