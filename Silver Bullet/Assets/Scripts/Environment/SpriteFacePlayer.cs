using UnityEngine;

public class SpriteFacePlayer : MonoBehaviour
{
    [SerializeField] private Transform player;

    private void Update()
    {
        transform.LookAt(player);
    }
}
