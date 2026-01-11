using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Singleton instance
    public static MainMenuManager Instance { get; private set; }

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Hides all menu panels.
    /// </summary>
    public void HideAll()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    /// <summary> 
    /// Shows a specific menu panel.
    /// </summary>
    public void ShowSpecific(GameObject gameObject)
    {
        gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(gameObject.transform.GetChild(0).gameObject);
    }

    /// <summary> 
    /// Starts the game
    /// </summary>
    public void PlayGame()
    {
        // Load the main game scene
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void OnQuit()
    {
        // Quit the application
        Application.Quit();

#if UNITY_EDITOR
        // Stop play mode if running inside the Unity editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}
