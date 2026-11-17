using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _settings;
    [SerializeField] private GameObject _developers;

    public void LoadNextScene()
    {
        SceneManager.LoadScene("Mine");
    }

    public void OpenShop()
    {
        SceneManager.LoadScene("BaseWorkshop");
    }

    public void OpenSettings()
    {
        _settings.SetActive(true);
        _mainMenu.SetActive(false);
    }

    public void CloseSettings()
    {
        _settings.SetActive(false);
        _mainMenu.SetActive(true);
    }

    public void ShowDevelopers()
    {
        _mainMenu.SetActive(false);
        _developers.SetActive(true);
    }

    public void CloseDevelopers()
    {
        _developers.SetActive(false);
        _mainMenu.SetActive(true);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("Mine");
    }

    public void Exit()
    {
        Debug.Log("Выход из игры...");

        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
