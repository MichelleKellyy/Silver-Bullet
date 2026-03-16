using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int playerHealth = 5;
    [SerializeField] private Image damageIndicator;

    private int initHealth;
    private void Start()
    {
        initHealth = playerHealth;
    }

    public void attack(int damage)
    {
        playerHealth -= damage;
        damageIndicator.color = new Color(1, 1, 1, (10 - playerHealth * 10 / initHealth) / 255f);
        if (playerHealth <= 0)
        {
            print("Die");
        }
    }
}
