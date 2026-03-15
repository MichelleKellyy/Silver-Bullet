using UnityEngine;

public class GunMech : MonoBehaviour
{
    public Transform cam;
    public Transform recallPoint;
    public GameObject bulletPrefab;
    public LayerMask hitMask;

    private Bullet currentBullet;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Shoot();
    }

    void Shoot()
    {
        if (currentBullet != null) return;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, Mathf.Infinity, hitMask))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.GetComponent<EnemyStats>().hit();
            }

            Vector3 towardsPlayer = (hit.point - cam.position).normalized * 0.5f;
            GameObject bulletObj = Instantiate(bulletPrefab, new Vector3(hit.point.x, hit.point.y > 3 ? hit.point.y : 3, hit.point.z) - towardsPlayer, Quaternion.identity);

            currentBullet = bulletObj.GetComponent<Bullet>();
            currentBullet.Init(this, recallPoint);
        }

    }

    public void ClearBulletLock(Bullet bullet)
    {
        if (currentBullet == bullet)
            currentBullet = null;
    }
}