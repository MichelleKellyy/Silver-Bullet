using UnityEngine;

public class ObjectShrinkOverTime : MonoBehaviour
{
    public float lifetime = 5f;
    public float shrinkSpeed = 1f;
    public bool dontShrink = false;

    private float timer = 0;
    private void Update()
    {
        if (dontShrink)
        {
            timer += Time.deltaTime;
            if (timer > lifetime)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;
            if (transform.localScale.magnitude < new Vector3(0.1f, 0.1f, 0.1f).magnitude)
            {
                Destroy(gameObject);
            }
        }
    }
}
