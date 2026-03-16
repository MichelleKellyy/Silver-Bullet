using UnityEngine;

public class GloveMech : MonoBehaviour
{
    public Transform cam;
    public LayerMask hitMask;
    public LayerMask hitMaskSpecific;

    public float initCooldown = 3;
    private float cooldown;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            UseGlove();
        }

        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }
    }

    void UseGlove()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, Mathf.Infinity, hitMask) && cooldown <= 0)
        {
            if (hit.collider.CompareTag("Bullet"))
            {
                hit.collider.GetComponent<Bullet>().StartRecall();
            }
            else if (hit.collider.CompareTag("Enemy"))
            {
                EnemyStats stats = hit.collider.GetComponent<EnemyStats>();
                if (stats.getArmoured())
                {
                    stats.hit();
                }
                else
                {
                    if (Physics.Raycast(cam.position, cam.forward, out hit, Mathf.Infinity, hitMaskSpecific))
                    {
                        if (hit.collider.CompareTag("Bullet"))
                        {
                            hit.collider.GetComponent<Bullet>().StartRecall();
                        }
                    }
                }
            }

            cooldown = initCooldown;
        }
    }
}
