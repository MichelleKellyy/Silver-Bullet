using UnityEngine;

public class GunMech : MonoBehaviour
{
    public CameraRotation cameraRotation;
    public Transform cam;
    public Transform recallPoint;
    public GameObject bulletPrefab;

    public LayerMask hitMask;

    public AudioSource shoot;
    public AudioSource reload;

    public Animator gunAnim;

    public ParticleSystem muzzleFlash;
    public GameObject hitEffect;

    private float initReloadCooldown = 2.4f;
    private float reloadCooldown;
    public float recoilAmount = 3;

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
            else if (hit.collider.CompareTag("Boss"))
            {
                hit.collider.GetComponent<BossStats>().TakeHit();
            }

            Vector3 towardsPlayer = (hit.point - cam.position).normalized * 0.5f;
            GameObject bulletObj = Instantiate(bulletPrefab, new Vector3(hit.point.x, hit.point.y > 3 ? hit.point.y : 3, hit.point.z) - towardsPlayer, Quaternion.identity);

            Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));

            currentBullet = bulletObj.GetComponent<Bullet>();
            currentBullet.Init(this, recallPoint);

            muzzleFlash.Play();
            shoot.Play();
            gunAnim.SetTrigger("Recoil");
            cameraRotation.xRot -= recoilAmount;
        }
    }

    public void ClearBulletLock(Bullet bullet)
    {
        if (currentBullet == bullet)
        {
            gunAnim.SetTrigger("Reload");
            reload.Play();
            reloadCooldown = initReloadCooldown;
            currentBullet = null;
        }
    }
}