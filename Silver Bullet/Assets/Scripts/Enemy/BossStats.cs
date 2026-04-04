using UnityEngine;
using System.Collections;

public class BossStats : MonoBehaviour
{
    [SerializeField] private int health = 15;
    [SerializeField] private float cooldownLength = 5f;
    [SerializeField] private Transform rotatePoint;
    [SerializeField] private AudioSource bones;
    [SerializeField] private AudioSource metal;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject deathEffect;

    private bool activated = true;

    public bool getActivated()
    {
        return activated;
    }

    private float cooldown = 0f;
    public void deactivate()
    {
        activated = false;
        GetComponent<Animator>().SetTrigger("Interrupt");
        GetComponent<Animator>().SetBool("Deactivate", true);
        GetComponent<BossAI>().cancelDash();
        cooldown = cooldownLength;
    }

    private void Update()
    {
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }
        else if (!activated)
        {
            GetComponent<Animator>().SetBool("Deactivate", false);
            activated = true;
        }
    }

    public void TakeHit()
    {
        if (!activated)
        {
            GetComponent<BossAI>().increaseSpeed();
            GetComponent<BossAI>().decreaseShootInterval();
            health -= 1;
            bones.Play();

            GetComponent<Rigidbody>().linearVelocity = -rotatePoint.forward * 10f;
            StartCoroutine(stopVelocity());

            if (health <= 0)
            {
                Instantiate(deathEffect, new Vector3(transform.position.x, 54f, transform.position.z), Quaternion.Euler(270, 180, 0));
                uiManager.playEndingSequence();
                Destroy(gameObject);
            }

            GetComponent<Animator>().SetBool("Deactivate", false);
            activated = true;
            cooldown = 0;
        }
        else
        {
            metal.Play();
        }
    }

    IEnumerator stopVelocity()
    {
        yield return new WaitForSeconds(0.25f);
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }
}