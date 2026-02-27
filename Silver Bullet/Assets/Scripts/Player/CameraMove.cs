using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] Transform cameraTarget;

    private void Update()
    {
        transform.position = cameraTarget.position;
        print("cam: " + cameraTarget.position);
        print("me: " + transform.position);
    }
}
