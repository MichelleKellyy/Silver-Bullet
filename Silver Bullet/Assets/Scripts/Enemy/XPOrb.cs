using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [Header("Orb Settings")]
    public int xpValue = 10;
    public float baseMoveSpeed = 5f;
    public float magnetRadius = 4f;

    [Header("Animation Settings")]
    public float bobAmplitude = 0.25f; 
    public float bobFrequency = 2f;   

    private Transform playerTransform;
    private bool isMagnetized = false;
    private float currentSpeed;
    private Vector3 startPosition;

    void Start()
    {
        currentSpeed = baseMoveSpeed;
        startPosition = transform.position; 

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float sqrDistanceToPlayer = (playerTransform.position - transform.position).sqrMagnitude;

        if (!isMagnetized && sqrDistanceToPlayer <= (magnetRadius * magnetRadius))
        {
            isMagnetized = true;
        }

        if (isMagnetized)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);
            currentSpeed += Time.deltaTime * 15f; 
        }
        else
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerXPManager>().AddXP(xpValue);
            Destroy(gameObject); 
        }
    }
}