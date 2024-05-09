using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class KillerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 2.0f;
    [SerializeField] float runSpeed = 5.0f;
    private Rigidbody rb;
    [SerializeField] private float groundDrag;
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    private Transform playerMidpointTransform;
    [SerializeField] float playerHeight = 2.5f;
    [SerializeField] LayerMask ground;
    private bool isGrounded;

    [Header("Camera")]
    [SerializeField] float mouseSensitivityX = 400.0f;
    [SerializeField] float mouseSensitivityY = 400.0f;
    [SerializeField] float upDownRange = 60.0f;
    private Transform playerCamTransform;
    private Camera playerCamObject;
    private float verticalRotation = 0f;
    private float closeClippingDistance = 0.3f;
    private float farClippingDistance = 0.5f;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerMidpointTransform = transform.Find("Midpoint");
            playerCamTransform = transform.Find("Camera");
            playerCamObject = playerCamTransform.GetComponent<Camera>();
            rb = GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            HandleMovement();
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            HandleCamera();
            CheckGrounded();
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(playerMidpointTransform.transform.position, Vector3.down, out RaycastHit hit, playerHeight * 0.5f + 0.3f, ground);

        if (!isGrounded) return;

        LayerMask curLayer = 1 << hit.collider.gameObject.layer;

        //Preform changes to drag here:
        rb.drag = groundDrag;
    }

    private void HandleMovement()
    {

        if (!isGrounded) return;

        // Movement
        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        if (moveDirection == Vector3.zero)
        {
            return;
        }

        float movementSpeed = walkSpeed;

        if (Input.GetKey(runKey))
        {
            playerCamObject.nearClipPlane = closeClippingDistance;
            movementSpeed = runSpeed;
        }
        else
        {
            playerCamObject.nearClipPlane = farClippingDistance;
        }

        rb.AddForce(moveDirection.normalized * movementSpeed * 10f, ForceMode.Force);

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limiting velocity to specified amount
        if(flatVel.magnitude > movementSpeed)
        {
            Vector3 limitVel = flatVel.normalized * movementSpeed;
            rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
        }
    }

    private void HandleCamera()
    {
        // Player rotation (left/right)
        float horizontalRotationInput = Input.GetAxisRaw("Mouse X") * mouseSensitivityX;
        float targetHorizontalRotation = transform.rotation.eulerAngles.y + horizontalRotationInput;

        transform.rotation = Quaternion.Euler(0, targetHorizontalRotation, 0);

        // Camera rotation (up/down)
        verticalRotation -= Input.GetAxisRaw("Mouse Y") * mouseSensitivityY;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);

        // Apply the vertical rotation within the clamped range
        playerCamTransform.transform.localEulerAngles = new Vector3(verticalRotation, playerCamTransform.transform.localEulerAngles.y, 0);
    }
}
