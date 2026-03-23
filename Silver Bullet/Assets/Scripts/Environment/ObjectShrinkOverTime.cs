using UnityEngine;

public class ObjectShrinkOverTime : MonoBehaviour
{
    public float shrinkSpeed = 1f;
    private void Update()
    {
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;
        if (transform.localScale.magnitude < new Vector3(0.1f, 0.1f, 0.1f).magnitude)
        {
            Destroy(gameObject);
        }
    }
}
