using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private Material armoured;
    [SerializeField] private Material notArmoured;

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
            GetComponent<MeshRenderer>().material = notArmoured;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
