using UnityEngine;

public class GearFollowPlayer : MonoBehaviour
{
    public Transform cam;
    public Transform loc;

    public float positionSmooth = 12f;
    public float rotationSmooth = 12f;

    void LateUpdate()
    {
        float posT = 1f - Mathf.Exp(-positionSmooth * Time.deltaTime);
        float rotT = 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, loc.position, posT);
        transform.rotation = Quaternion.Slerp(transform.rotation, loc.rotation, rotT);
    }
}
