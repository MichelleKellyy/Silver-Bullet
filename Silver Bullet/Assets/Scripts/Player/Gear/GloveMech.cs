using UnityEngine;

public class GloveMech : MonoBehaviour
{
    public Transform cam;
    public LayerMask hitMask;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            UseGlove();
        }
    }

    void UseGlove()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, Mathf.Infinity, hitMask))
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
            }
        }
    }
}
