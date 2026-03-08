using UnityEngine;

public class GunGloveMech : MonoBehaviour
{
    public Transform firePoint;
    public Transform recallPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 25f;

    private Bullet currentBullet;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Shoot();

        if (Input.GetMouseButtonDown(1))
            RecallBullet();
    }

    void Shoot()
    {
        if (currentBullet != null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        currentBullet = bulletObj.GetComponent<Bullet>();
        currentBullet.Launch(this, recallPoint, firePoint.forward, bulletSpeed);
    }

    void RecallBullet()
    {
        if (currentBullet == null) return;
        currentBullet.StartRecall();
    }

    public void ClearBulletLock(Bullet bullet)
    {
        if (currentBullet == bullet)
            currentBullet = null;
    }
}