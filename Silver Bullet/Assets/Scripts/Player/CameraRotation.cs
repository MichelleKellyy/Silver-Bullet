using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cam;
    [SerializeField] Transform orientation;

    [Header("Look Variables")]
    [SerializeField] float mouseSensitivity = 100f;

    [HideInInspector] public float xRot;
    [HideInInspector] public float yRot = -90;

    private float multiplier = 0.01f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        getInput();
        look();
    }

    private void getInput()
    {
        float x = Input.GetAxisRaw("Mouse X");
        float y = Input.GetAxisRaw("Mouse Y");

        yRot += x * mouseSensitivity * multiplier;
        xRot -= y * mouseSensitivity * multiplier;

        xRot = Mathf.Clamp(xRot, -90f, 90f);
    }

    private void look()
    {
        cam.transform.rotation = Quaternion.Euler(xRot, yRot, 0);
        orientation.transform.rotation = Quaternion.Euler(0, yRot, 0);
    }
}
