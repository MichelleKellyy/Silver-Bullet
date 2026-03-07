using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    private Camera cam;

    private GunGloveMech ownerGun;
    private Transform recallTarget;

    private bool isRecalling = false;

    public float recallSpeed = 30f;
    public float catchDistance = 0.3f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        cam = Camera.main;
    }

    public void Launch(GunGloveMech gun, Transform recallPoint, Vector3 shootDirection, float speed)
    {
        ownerGun = gun;
        recallTarget = recallPoint;

        rb.linearVelocity = shootDirection * speed;
    }

    public void StartRecall()
    {
        isRecalling = true;
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;
    }

    void Update()
    {
        if (cam != null)
        {
            transform.forward = cam.transform.forward;
        }

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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            rb.linearVelocity = Vector3.zero;
        }
    }
}