using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    public bool isArcher;

    [SerializeField] private Material armoured;
    [SerializeField] private Material armouredArcher;
    [SerializeField] private Material notArmoured;
    [SerializeField] private Material notArmouredArcher;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip armourHitClip;
    [SerializeField] private AudioClip unarmouredHitClip;

    [SerializeField] private GameObject xpOrbPrefab;

    private bool isArmoured = true;

    public bool getArmoured()
    {
        return isArmoured;
    }

    public void hit()
    {
        if (isArmoured)
        {
            isArmoured = false;
            if (isArcher)
            {
                GetComponentInChildren<MeshRenderer>().material = notArmouredArcher;
            }
            else
            {
                GetComponentInChildren<MeshRenderer>().material = notArmoured;
            }

            if (audioSource != null && armourHitClip != null)
                audioSource.PlayOneShot(armourHitClip);
        }
        else
        {
            if (audioSource != null && unarmouredHitClip != null)
                StartCoroutine(playSoundOnDeath());
        }
    }

    private IEnumerator playSoundOnDeath()
    {
        audioSource.PlayOneShot(unarmouredHitClip);

        GetComponentInChildren<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<EnemyNavAI>().enabled = false;

        while (audioSource.isPlaying)
        {
            yield return null;
        }
        Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}