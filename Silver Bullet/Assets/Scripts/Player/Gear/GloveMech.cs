using UnityEngine;

public class GloveMech : MonoBehaviour
{
    public GameObject skeletonArmour;
    public GameObject skeletonArcherArmour;
    public Transform cam;
    public LayerMask hitMask;
    public LayerMask hitMaskSpecific;
    public Animator gloveAnim;
    public ParticleSystem electricity;
    public AudioSource gloveUse;

    public float initCooldown = 3f;
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
                    GameObject armour = null;
                    if (stats.isArcher)
                    {
                        armour = Instantiate(skeletonArcherArmour, hit.transform.position, Quaternion.LookRotation(cam.transform.position - hit.transform.position));
                    }
                    else
                    {
                        armour = Instantiate(skeletonArmour, hit.transform.position, Quaternion.LookRotation(cam.transform.position - hit.transform.position));
                    }
                    armour.GetComponent<Rigidbody>().AddForce(transform.forward * -15, ForceMode.Impulse);
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

            gloveUse.Play();
            gloveAnim.SetTrigger("UseGlove");
            electricity.Play();
            cooldown = initCooldown;
        }
    }
}
