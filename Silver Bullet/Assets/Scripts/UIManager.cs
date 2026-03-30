using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Needed for Buttons and Images
using TMPro; // Needed for TextMeshPro

public class UIManager : MonoBehaviour
{
    public AudioSource pauseSound;

    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject hud;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject levelUpPanel;

    [Header("Level Up Buttons & Icons")]
    public Button skillBtn1;

    public Button skillBtn2;

    [Header("Player Control")]
    public CameraRotation cameraRotation;
    public MonoBehaviour playerMovement;
    public GunMech gunMech;

    private bool isPaused = false;
    private bool gameStarted = false;
    private bool gameOver = false;
    private bool isLevelingUp = false;

    private static bool restartIntoGame = false;
    private PlayerStats playerStats;
    private GloveMech gloveMech;

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        gloveMech = FindObjectOfType<GloveMech>();

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
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraRotation != null) cameraRotation.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (gunMech != null) gunMech.enabled = false;
    }

    private void Update()
    {
        if (gameStarted && !gameOver && !isLevelingUp && Input.GetKeyDown(KeyCode.Escape))
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
        isLevelingUp = false;

        startPanel.SetActive(false);
        hud.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraRotation != null) cameraRotation.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (gunMech != null) gunMech.enabled = true;
    }

    public void PauseGame()
    {
        isPaused = true;

        startPanel.SetActive(false);
        hud.SetActive(false);
        pausePanel.SetActive(true);
        gameOverPanel.SetActive(false);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraRotation != null) cameraRotation.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (gunMech != null) gunMech.enabled = false;
    }

    public void ResumeGame()
    {
        isPaused = false;
        isLevelingUp = false;

        startPanel.SetActive(false);
        hud.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraRotation != null) cameraRotation.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (gunMech != null) gunMech.enabled = true;
    }

    public void ShowGameOver()
    {
        gameOver = true;
        isPaused = false;
        isLevelingUp = false;

        startPanel.SetActive(false);
        hud.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(true);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraRotation != null) cameraRotation.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (gunMech != null) gunMech.enabled = false;
    }

    public void ShowLevelUpScreen()
    {
        isLevelingUp = true;

        hud.SetActive(false);
        levelUpPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraRotation != null) cameraRotation.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (gunMech != null) gunMech.enabled = false;

        // Full heal
        skillBtn1.onClick.RemoveAllListeners();
        skillBtn1.GetComponentInChildren<TextMeshProUGUI>().text = "Restore Health";
        skillBtn1.onClick.AddListener(() => ApplyUpgrade(0));

        // Increase recharge rate
        skillBtn2.onClick.RemoveAllListeners();
        skillBtn2.GetComponentInChildren<TextMeshProUGUI>().text = "Increase Recharge Rate";
        skillBtn2.onClick.AddListener(() => ApplyUpgrade(1));

        // Increase movement speed

        // Increase max health

    }

    private void ApplyUpgrade(int upgradeID)
    {
        if (upgradeID == 0) 
        {
            if (playerStats != null) playerStats.RestoreHealth();
        }
        else if (upgradeID == 1) 
        {
            if (gloveMech != null) gloveMech.UpgradeRechargeRate();
        }

        ResumeGame(); 
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