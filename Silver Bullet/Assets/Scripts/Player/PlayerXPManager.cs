using UnityEngine;
using UnityEngine.UI;

public class PlayerXPManager : MonoBehaviour
{
    [Header("Experience")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 20;
    

    private PlayerStats stats;

    public Image XPBar_FL;


    void Start()
    {
        stats = GetComponent<PlayerStats>();
        XPBar_FL.fillAmount = currentXP;
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        XPBar_FL.fillAmount = (float)currentXP / xpToNextLevel;

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

private void LevelUp()
    {
        currentXP -= xpToNextLevel; 
        currentLevel++;
        xpToNextLevel += 10;
        XPBar_FL.fillAmount = (float)currentXP / xpToNextLevel;
        FindFirstObjectByType<UIManager>().ShowLevelUpScreen();
    }
}