using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private Material armoured;
    [SerializeField] private Material notArmoured;

    private bool isArmoured = true;

    public AudioSource armourHit;
    public AudioSource unarmouredHit;

    public bool getArmoured()
    {
        return isArmoured;
    }

    public void hit()
    {
        if (isArmoured)
        {
            isArmoured = false;
            GetComponentInChildren<MeshRenderer>().material = notArmoured;
            armourHit.Play();
            Debug.Log("armour hit");
;
        }
        else
        {
            unarmouredHit.Play();
            Destroy(gameObject);
            Debug.Log("enemy killed");

        }
    }
}
