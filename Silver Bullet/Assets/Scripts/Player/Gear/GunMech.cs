using UnityEngine;

public class GunMech : MonoBehaviour
{
    public Transform cam;
    public Transform recallPoint;
    public GameObject bulletPrefab;

    public LayerMask hitMask;

    public AudioSource shoot;
    public AudioSource reload;

    public Animator reloadAnim;
    private float initReloadCooldown = 2.4f;
    private float reloadCooldown;

    private Bullet currentBullet;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Shoot();

        if (reloadCooldown > 0)
        {
            reloadCooldown -= Time.deltaTime;
        }
    }

    void Shoot()
    {
        if (currentBullet != null) return;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, Mathf.Infinity, hitMask) && reloadCooldown <= 0)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.GetComponent<EnemyStats>().hit();
            }

            Vector3 towardsPlayer = (hit.point - cam.position).normalized * 0.5f;
            GameObject bulletObj = Instantiate(bulletPrefab, new Vector3(hit.point.x, hit.point.y > 3 ? hit.point.y : 3, hit.point.z) - towardsPlayer, Quaternion.identity);

            currentBullet = bulletObj.GetComponent<Bullet>();
            currentBullet.Init(this, recallPoint);

            shoot.Play();
        }
    }

    public void ClearBulletLock(Bullet bullet)
    {
        if (currentBullet == bullet)
        {
            reloadAnim.SetTrigger("Reload");
            reload.Play();
            reloadCooldown = initReloadCooldown;
            currentBullet = null;
        }
    }
}