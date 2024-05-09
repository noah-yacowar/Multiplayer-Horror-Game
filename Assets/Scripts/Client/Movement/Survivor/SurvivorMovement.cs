using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class SurvivorMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 2.0f;
    [SerializeField] float runSpeed = 5.0f;
    [SerializeField] float rotationSpeed = 5.0f;
    private SurvivorAnimationStateController survivorAnimatorController;
    private Rigidbody rb;
    [SerializeField] private float groundDrag;
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    private Transform playerMidpointTransform;
    [SerializeField] float playerHeight = 2f;
    [SerializeField] LayerMask ground;
    private bool isGrounded;

    [Header("Rotation")]
    private Transform playerCamCenterTransform;
    private Transform playerModelTransform;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            survivorAnimatorController = GetComponent<SurvivorAnimationStateController>();
            playerMidpointTransform = transform.Find("Midpoint");
            playerModelTransform = transform.Find("Armature");
            rb = GetComponent<Rigidbody>();
            playerCamCenterTransform = transform.Find("Camera Center");
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
            CheckGrounded();
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(playerMidpointTransform.transform.position, Vector3.down, out RaycastHit hit, playerHeight * 0.5f + 0.3f, ground);

        if (!isGrounded) return;

        //Preform changes to drag here:
        rb.drag = groundDrag;
    }

    private void HandleMovement()
    {
        if (!isGrounded || survivorAnimatorController.IsAnimatorInRepairingState()) return;

        // Movement
        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        Vector3 moveDirection = (playerCamCenterTransform.forward * verticalInput + playerCamCenterTransform.right * horizontalInput).normalized;

        if (moveDirection == Vector3.zero)
        {
            return;
        }

        float movementSpeed = walkSpeed;

        if (Input.GetKey(runKey))
        {
            movementSpeed = runSpeed;
        }



        moveDirection.y = 0f;

        rb.AddForce(moveDirection * movementSpeed * 10f, ForceMode.Force);

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limiting velocity to specified amount
        if (flatVel.magnitude > movementSpeed)
        {
            Vector3 limitVel = flatVel.normalized * movementSpeed;
            rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
        }

        // Calculate target rotation only around the y-axis
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        // Extract only the y-component of the rotation
        float targetYRotation = targetRotation.eulerAngles.y;
        // Smoothly interpolate the y-rotation
        float newYRotation = Mathf.LerpAngle(playerModelTransform.transform.eulerAngles.y, targetYRotation, rotationSpeed * Time.deltaTime);
        // Apply the new rotation
        playerModelTransform.transform.rotation = Quaternion.Euler(-90f, newYRotation, 0f);
    }
}

