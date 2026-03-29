using UnityEngine;
using System.Collections;

public class PlayerCollider : MonoBehaviour
{
    [SerializeField] private Animator teleport;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Teleporter"))
        {
            StartCoroutine("Teleport");
        }
        if (collision.CompareTag("Key"))
        {
            GetComponentInParent<PlayerStats>().addKey();
            Destroy(collision.gameObject);
        }
    }

    IEnumerator Teleport()
    {
        GetComponent<PlayerMove>().enabled = false;
        teleport.SetTrigger("Teleport");
        yield return new WaitForSeconds(5f);
        GetComponent<PlayerMove>().enabled = true;
    }
}
