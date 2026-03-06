using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform orientation;

    [Header("Movement Variables")]
    [SerializeField] float airMultiplier = 0.4f;
    [SerializeField] float playerHeight = 2f;
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 8f;
    [SerializeField] float acceleration = 10f;

    [Header("Jump Variables")]
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;

    [Header("Drag")]
    [SerializeField] float moveDrag = 5f;
    [SerializeField] float airDrag = 2f;

    [Header("Keybinds")]
    [SerializeField] KeyCode runKey = KeyCode.LeftShift;

    [Header("Head Bob")]
    [SerializeField] Transform playerCam;
    [SerializeField] float headBobAmount;
    [SerializeField] float headBobSpeed;

    [Header("Sounds")]
    [SerializeField] AudioSource[] Steps;

    // GENERAL
    private float speed = 100f;
    private float horizontalMove;
    private float verticalMove;
    private Vector3 movement;

    private Vector3 slopeMoveDirection;
    private RaycastHit slopeHit;

    private float bobAmt = 0;
    private float stepCoolDown = 0f;
    private float playerCamOriginalPositionY;


    private void Awake()
    {
        playerCamOriginalPositionY = playerCam.localPosition.y;
    }
    private void Update()
    {
        getInput();
        drag();
        controlSpeed();

        if (isGrounded())
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }
    }
    private void LateUpdate()
    {
        headBob();
    }
    private void FixedUpdate()
    {
        movePlayer();
    }

    private bool onSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool isGrounded()
    {
        if (Physics.CheckSphere(groundCheck.position, groundDistance, groundMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void getInput()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal");
        verticalMove = Input.GetAxisRaw("Vertical");

        movement = (orientation.forward * verticalMove + orientation.right * horizontalMove).normalized;
        slopeMoveDirection = Vector3.ProjectOnPlane(movement, slopeHit.normal);
    }

    private void controlSpeed()
    {
        if (Input.GetKey(runKey) && isGrounded())
        {
            speed = Mathf.Lerp(speed, runSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            speed = Mathf.Lerp(speed, walkSpeed, acceleration * Time.deltaTime);
        }
    }

    private void drag()
    {
        if (isGrounded())
        {
            rb.linearDamping = moveDrag;
        }
        else
        {
            rb.linearDamping = airDrag;
        }
    }

    private void movePlayer()
    {
        if (isGrounded() && !onSlope()) //Ground
        {
            rb.AddForce(movement * speed, ForceMode.Acceleration);
        }
        else if (isGrounded() && onSlope()) //Slope
        {
            rb.AddForce(slopeMoveDirection * speed, ForceMode.Acceleration);
        }
        else //Air
        {
            rb.AddForce(movement * speed * airMultiplier, ForceMode.Acceleration);
        }
    }

    int previousStep = -1;
    private void headBob()
    {
        if (Mathf.Abs(horizontalMove) > 0 || Mathf.Abs(verticalMove) > 0)
        {
            bobAmt += ((Mathf.Abs(horizontalMove) + Mathf.Abs(verticalMove)) / (Mathf.Abs(horizontalMove) + Mathf.Abs(verticalMove))) * (Input.GetKey(runKey) ? headBobSpeed * 1.45f : headBobSpeed) * Time.deltaTime;
        }
        else
        {
            float nearestPI = (int)(bobAmt / Mathf.PI) * Mathf.PI;
            bobAmt = Mathf.Lerp(bobAmt, nearestPI, 0.01f);
        }

        float sinVal = Mathf.Sin(bobAmt);
        playerCam.localPosition = new Vector3(playerCam.localPosition.x, playerCamOriginalPositionY + (sinVal * headBobAmount), playerCam.localPosition.z);

        stepCoolDown -= Time.deltaTime;
        if ((Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f) && stepCoolDown < 0f)
        {
            int choice = Random.Range(0, Steps.Length);
            while (choice == previousStep)
            {
                choice = Random.Range(0, Steps.Length);
            }
            previousStep = choice;
            Steps[choice].Play();
            stepCoolDown = Input.GetKey(runKey) ? headBobSpeed / 13 * 0.69f : headBobSpeed / 13;
        }
    }
}
