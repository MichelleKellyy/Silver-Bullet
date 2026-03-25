using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    public void SetDamage(int value)
    {
        damage = value;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerStats stats = collision.collider.GetComponentInParent<PlayerStats>();
            stats.attack(damage);
        }

        Destroy(gameObject);
    }

    void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.05f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity.normalized);
        }
    }
}