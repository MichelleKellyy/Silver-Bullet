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
        Debug.Log("Gained " + amount + " XP! Total: " + currentXP + "/" + xpToNextLevel);

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel; 
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 2); 

        if (stats != null)
        {
            stats.RestoreHealthOnLevelUp();
        }

        // TODO: Trigger the UIManager to show the Skill Selection screen
    }
}