using UnityEngine;
using System.Collections;

public class PlayerCollider : MonoBehaviour
{
    [SerializeField] private Animator teleport;
    [SerializeField] private GameObject bossRoom;
    [SerializeField] private AudioSource normalMusic;
    [SerializeField] private AudioSource bossMusic;
    [SerializeField] private GameObject bossObj;
    [SerializeField] private GunMech gunMech;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Teleporter"))
        {
            StartCoroutine(fadeOutInMusic());
            StartCoroutine(Teleport());
        }
        if (collision.CompareTag("Key"))
        {
            GetComponentInParent<PlayerStats>().addKey();
            Destroy(collision.gameObject);
        }
        if (collision.CompareTag("Door"))
        {
            if (GetComponentInParent<PlayerStats>().HasThreeKeys())
            {
                collision.GetComponent<Animator>().SetTrigger("Open");
                collision.GetComponent<AudioSource>().Play();
                GetComponentInParent<PlayerStats>().removeKeys();
            }
        }
    }

    IEnumerator Teleport()
    {
        gunMech.remoteCallBullet();

        GetComponent<PlayerMove>().enabled = false;
        teleport.SetTrigger("Teleport");
        yield return new WaitForSeconds(2f);

        transform.position = bossRoom.transform.position;
        GetComponent<Rigidbody>().position = bossRoom.transform.position;

        yield return new WaitForSeconds(3f);
        GetComponent<PlayerMove>().enabled = true;

        yield return new WaitForSeconds(0.5f);
        bossObj.SetActive(true);
    }

    private IEnumerator fadeOutInMusic()
    {
        while (normalMusic.volume > 0)
        {
            normalMusic.volume -= 0.01f;
            yield return new WaitForSeconds(0.05f);
        }
        normalMusic.Stop();
        bossMusic.Play();
        while (bossMusic.volume < 0.25f)
        {
            bossMusic.volume += 0.01f;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
