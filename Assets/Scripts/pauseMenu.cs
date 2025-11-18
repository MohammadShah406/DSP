using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public bool isPaused = false;

    public static PauseMenu Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple PauseManagers instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        isPaused = false;
    }

    private void Start()
    {
        pauseMenuUI.SetActive(isPaused);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            pauseMenuUI.SetActive(isPaused);
            Time.timeScale = isPaused ? 0 : 1;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1;
    }
}

