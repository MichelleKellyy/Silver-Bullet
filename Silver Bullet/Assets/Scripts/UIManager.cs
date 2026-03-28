using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public AudioSource pauseSound;

    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject hud;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("Player Control")]
    public CameraRotation cameraRotation;
    public MonoBehaviour playerMovement;
    public GunMech gunMech;

    private bool isPaused = false;
    private bool gameStarted = false;
    private bool gameOver = false;

    private static bool restartIntoGame = false;

    private void Start()
    {
        if (restartIntoGame)
        {
            restartIntoGame = false;
            StartGame();
            return;
        }

        startPanel.SetActive(true);
        hud.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraRotation != null)
            cameraRotation.enabled = false;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (gunMech != null)
            gunMech.enabled = false;
    }

    private void Update()
    {
        if (gameStarted && !gameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
            {
                PauseGame();
                pauseSound.Play();
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

        if (cameraRotation != null)
            cameraRotation.enabled = true;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (gunMech != null)
            gunMech.enabled = true;
    }

    public void PauseGame()
    {
        isPaused = true;

        startPanel.SetActive(false);
        hud.SetActive(false);
        pausePanel.SetActive(true);
        gameOverPanel.SetActive(false);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraRotation != null)
            cameraRotation.enabled = false;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (gunMech != null)
            gunMech.enabled = false;
    }

    public void ResumeGame()
    {
        isPaused = false;

        startPanel.SetActive(false);
        hud.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraRotation != null)
            cameraRotation.enabled = true;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (gunMech != null)
            gunMech.enabled = true;
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

        if (cameraRotation != null)
            cameraRotation.enabled = false;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (gunMech != null)
            gunMech.enabled = false;
    }

    public void RestartGame()
    {
        restartIntoGame = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}