using UnityEngine;

public class PlayerXPManager : MonoBehaviour
{
    [Header("Experience")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 20;

    private PlayerStats stats;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

private void LevelUp()
    {
        currentXP -= xpToNextLevel; 
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f); 

        FindObjectOfType<UIManager>().ShowLevelUpScreen();
    }
}