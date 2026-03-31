using UnityEngine;

public class BossStats : MonoBehaviour
{
    [SerializeField] private int health = 15;

    public void TakeHit(int damage = 1)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}