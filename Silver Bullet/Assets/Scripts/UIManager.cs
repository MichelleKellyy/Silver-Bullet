using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SkillOption
{
    public string skillName;
    public int skillID;
}

public class UIManager : MonoBehaviour
{
    public AudioSource pauseSound;
    public AudioSource bossMusic;
    public AudioSource normalMusic;

    [Header("UI Panels")]
    public Animator blackScreen;
    public GameObject startPanel;
    public GameObject hud;
    public GameObject pausePanel;
    public GameObject controlPanel;
    public GameObject gameOverPanel;
    public GameObject levelUpPanel;
    public GameObject finalButtons;
    public TextMeshProUGUI dialogue;

    [Header("Skill Pool")]
    public List<SkillOption> availableSkills;

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

    // private static bool restartIntoGame = false;
    private PlayerStats playerStats;
    private GloveMech gloveMech;
    private PlayerMove PlayerMove;

    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        gloveMech = FindFirstObjectByType<GloveMech>();
        PlayerMove = FindFirstObjectByType<PlayerMove>();

        // This doesn't function with the new intro, not sure if we want to make it work or not
        /*if (restartIntoGame)
        {
            restartIntoGame = false;
            StartGame();
            return;
        }*/

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
        StartCoroutine(startGame());
    }

    private IEnumerator startGame()
    {
        startPanel.SetActive(false);
        Time.timeScale = 1f;

        blackScreen.gameObject.SetActive(true);
        dialogue.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        dialogue.GetComponent<Animator>().SetTrigger("FadeIn");
        dialogue.text = "You wake up in a dark dungeon, unable to remember who you are.\n\nAround you are scattered items.\n\nAmong them you find a map, a glowing glove, a pistol, and a single bullet.\n\n\nPress 'Left Click' to continue.";

        yield return new WaitUntil(() => Input.GetMouseButton(0));

        dialogue.GetComponent<Animator>().SetTrigger("FadeOut");
        yield return new WaitForSeconds(1f);
        dialogue.text = "Controls\n\n'Left Click' : Shoot\n'Right Click' : Use glove on metal objects\n'Left Shift' : Run\n'Space' + 'A' or 'D' : Dash\n'M' : Open map\n'Escape' : Pause\n\nPress 'Left Click' to continue.";
        dialogue.GetComponent<Animator>().SetTrigger("FadeIn");

        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => Input.GetMouseButton(0));

        dialogue.GetComponent<Animator>().SetTrigger("FadeOut");
        blackScreen.SetTrigger("FadeOut");

        gameStarted = true;
        gameOver = false;
        isPaused = false;
        isLevelingUp = false;

        hud.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraRotation != null) cameraRotation.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (gunMech != null) gunMech.enabled = true;
    }

    public void playEndingSequence()
    {
        StartCoroutine(fadeOutInMusic());
        StartCoroutine(winGame());
    }

    private IEnumerator fadeOutInMusic()
    {
        while (bossMusic.volume > 0)
        {
            bossMusic.volume -= 0.01f;
            yield return new WaitForSeconds(0.05f);
        }
        bossMusic.Stop();
        normalMusic.Play();
        while (normalMusic.volume < 0.25f)
        {
            normalMusic.volume += 0.01f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator winGame()
    {
        yield return new WaitForSeconds(3f);

        gameOver = true;
        hud.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraRotation != null) cameraRotation.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (gunMech != null) gunMech.enabled = false;

        blackScreen.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1f);
        dialogue.GetComponent<Animator>().SetTrigger("FadeIn");
        dialogue.text = "The dungeon falls silent as the final monster stops moving.\n\n\nPress 'Left Click' to continue.";

        yield return new WaitUntil(() => Input.GetMouseButton(0));

        dialogue.GetComponent<Animator>().SetTrigger("FadeOut");
        yield return new WaitForSeconds(1f);
        dialogue.text = "There is nowhere else for you to go.\n\nWhat comes next is up to you.";
        dialogue.GetComponent<Animator>().SetTrigger("FadeIn");

        finalButtons.SetActive(true);
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

    public void ShowControls()
    {
        controlPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void HideControls()
    {
        controlPanel.SetActive(false);
        pausePanel.SetActive(true);
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

        List<SkillOption> tempPool = new List<SkillOption>(availableSkills);

        if (playerStats != null && !playerStats.IsMissingHealth())
        {
            tempPool.RemoveAll(skill => skill.skillID == 0);
        }

        int randomIdx1 = Random.Range(0, tempPool.Count);
        SkillOption choice1 = tempPool[randomIdx1];
        tempPool.RemoveAt(randomIdx1);

        int randomIdx2 = Random.Range(0, tempPool.Count);
        SkillOption choice2 = tempPool[randomIdx2];

        skillBtn1.onClick.RemoveAllListeners();
        skillBtn1.GetComponentInChildren<TextMeshProUGUI>().text = choice1.skillName;
        skillBtn1.onClick.AddListener(() => ApplyUpgrade(choice1.skillID));

        skillBtn2.onClick.RemoveAllListeners();
        skillBtn2.GetComponentInChildren<TextMeshProUGUI>().text = choice2.skillName;
        skillBtn2.onClick.AddListener(() => ApplyUpgrade(choice2.skillID));
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
        else if (upgradeID == 2)
        {
            if (PlayerMove != null) PlayerMove.IncreaseSpeed();
        }
        else if (upgradeID == 3)
        {
            if (playerStats != null) playerStats.IncreaseMaxHealth();
        }

        ResumeGame(); 
    }

    public void RestartGame()
    {
        // restartIntoGame = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}