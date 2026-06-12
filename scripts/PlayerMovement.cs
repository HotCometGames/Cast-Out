using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static Transform instance;
    public EntityStatHandler entityStats;
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public float jumpForce = 5f;
    public float moveToJumpRatio = 0.5f;
    public float gravity = -9.81f;
    public bool inputEnabled = true;

    //debug DELETE LATER
    public float debugSpeedMultiplier = 20f;
    public float debugAppliedMultiplier = 1f;

    public Transform cameraTransform;

    private Rigidbody rb;
    private float xRotation = 0f;

    void Awake()
    {
        instance = this.transform;
        moveToJumpRatio = jumpForce / entityStats.maxSpeed;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Mouse look
        if (inputEnabled)
        {
            CHECKFORDEBUG();
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            // Movement
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            move *= entityStats.currentSpeed * debugAppliedMultiplier;

            Vector3 velocity = rb.velocity;
            velocity.x = move.x;
            velocity.z = move.z;
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
            float currentSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
            ItemHoldingUIScript.SetSpeed(currentSpeed / (entityStats.currentSpeed * debugAppliedMultiplier));

            // Jumping
            if (isGroundedCheck() && Input.GetButton("Jump"))
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z); // Reset vertical velocity
                //rb.AddForce(Vector3.up * moveToJumpRatio * entityStats.maxSpeed, ForceMode.Impulse);
            }
        } else
        {
            ItemHoldingUIScript.SetSpeed(0f);
        }
    }

    void FixedUpdate()
    {
        // Gravity is handled by Rigidbody
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                break;
            }
            else if (contact.normal.y > 0.1f && contact.normal.y <= 0.5f)
            {
                // Edge detected: apply sliding and slow down
                Vector3 slideDir = Vector3.ProjectOnPlane(rb.velocity, contact.normal);
                rb.velocity = slideDir * 0.5f; // Reduce speed when sliding
            }
        }
    }

    bool isGroundedCheck()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.6f);
    }

    void CHECKFORDEBUG()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            debugAppliedMultiplier = debugAppliedMultiplier == 1f ? debugSpeedMultiplier : 1f;
            Debug.Log("Debug Speed Multiplier: " + debugAppliedMultiplier);
        }
    }
}
