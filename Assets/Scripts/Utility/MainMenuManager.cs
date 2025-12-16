using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Target scene name")]
    public string gameSceneName = "GamePlay";

    public void NewGame()
    {
        PlayerDataManager.Instance.ResetAllData();
        SceneManager.LoadScene(gameSceneName);
    }
    public void ContinueGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
