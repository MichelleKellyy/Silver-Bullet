using UnityEngine;

public class Bullet : MonoBehaviour
{
    private GunMech ownerGun;
    private Transform recallTarget;

    private bool isRecalling = false;

    public float recallSpeed = 30f;
    public float catchDistance = 0.3f;

    public void Init(GunMech gun, Transform recallPoint)
    {
        ownerGun = gun;
        recallTarget = recallPoint;
    }

    public void StartRecall()
    {
        isRecalling = true;
    }

    void Update()
    {
        if (isRecalling)
        {
            Vector3 direction = (recallTarget.position - transform.position).normalized;
            transform.position += direction * recallSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, recallTarget.position) <= catchDistance)
            {
                ownerGun.ClearBulletLock(this);
                Destroy(gameObject);
            }
        }
    }
}