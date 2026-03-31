using UnityEngine;

public class BossSword : MonoBehaviour
{
    [SerializeField] private int swordDamage = 2;
    private bool canDamage = false;

    public void SetCanDamage(bool value)
    {
        canDamage = value;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!canDamage) return;

        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponentInParent<PlayerStats>();
            if (stats != null)
            {
                stats.attack(swordDamage);
            }
            canDamage = false;
        }
    }
}