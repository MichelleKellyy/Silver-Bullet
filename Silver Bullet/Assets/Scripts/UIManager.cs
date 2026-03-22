using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject hud;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    private bool isPaused = false;
    private bool gameStarted = false;
    private bool gameOver = false;

    void Start()
    {
        startPanel.SetActive(true);
        hud.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (gameStarted && !gameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        gameOver = false;
        isPaused = false;

        startPanel.SetActive(false);
        hud.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void PauseGame()
    {
        isPaused = true;

        pausePanel.SetActive(true);
        hud.SetActive(false);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);
        hud.SetActive(true);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowGameOver()
    {
        gameOver = true;
        isPaused = false;

        startPanel.SetActive(false);
        hud.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}