using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int playerHealth = 5;
    [SerializeField] private Image damageIndicator;
    [SerializeField] private TextMeshProUGUI healthUI;

    public AudioSource damageSound;

    private int numKeys = 0;
    [SerializeField] GameObject key1;
    [SerializeField] GameObject key2;
    [SerializeField] GameObject key3;

    private int initHealth;
    private void Start()
    {
        initHealth = playerHealth;
    }

    public void attack(int damage)
    {
        damageSound.Play();

        playerHealth -= damage;
        damageIndicator.color = new Color(1, 1, 1, (10 - playerHealth * 10 / initHealth) / 255f);
        healthUI.text = (Mathf.Round(playerHealth / (float)initHealth * 100)).ToString() + "%";
        if (playerHealth <= 0)
        {
            FindObjectOfType<UIManager>().ShowGameOver();
        }
    }

    public void addKey()
    {
        numKeys += 1;
        if (!key1.activeSelf)
        {
            key1.SetActive(true);
        }
        else if (!key2.activeSelf)
        {
            key2.SetActive(true);
        }
        else if (!key3.activeSelf)
        {
            key3.SetActive(true);
        }
    }

    public void RestoreHealth()
    {
        playerHealth = initHealth;

        if (damageIndicator != null)
        {
            damageIndicator.color = new Color(1, 1, 1, 0f); 
        }

        if (healthUI != null)
        {
            healthUI.text = "100%";
        }
    }

    public void IncreaseMaxHealth()
    {
        initHealth += 20;
        playerHealth += 20;
        
        healthUI.text = (Mathf.Round(playerHealth / (float)initHealth * 100)).ToString() + "%";
        damageIndicator.color = new Color(1, 1, 1, (10 - playerHealth * 10 / initHealth) / 255f);
    }
}
