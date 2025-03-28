using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform playerCamera;

    [Header("Movement Speeds")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2f;
    private float currentSpeed;
    private bool isSprinting;

    [Header("Jump & Gravity")]
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    private float verticalVelocity;

    [Header("Crouch Settings")]
    public float crouchHeightReduction = 0.5f;
    public float crouchCameraOffset = 0.5f;
    private bool isCrouching;
    private float defaultControllerHeight;
    private Vector3 defaultControllerCenter;

    [Header("Mouse Look Settings")]
    public float lookSensitivity = 1f;
    private float cameraPitch = 0f;
    private Vector2 lookInput;

    [Header("Head Bob & Landing Bob Settings")]
    public float bobSpeed = 14f;
    public float bobAmount = 0.05f;
    private float bobTimer = 0f;
    public float landingBobIntensity = 0.1f;
    public float landingBobDecaySpeed = 10f;
    private float landingBobOffset = 0f;
    private bool landingTriggered = false;

    [Header("Camera Base Position")]
    private Vector3 defaultCameraLocalPos;

    private Vector2 moveInput;
    private PlayerInputActions.PlayerInputActions playerInput;
    private bool wasGrounded = true;

    private void Awake()
    {
        controller ??= GetComponent<CharacterController>();

        //Store default values for crouching and camera position.
        defaultControllerHeight = controller.height;
        defaultControllerCenter = controller.center;
        if (playerCamera != null)
            defaultCameraLocalPos = playerCamera.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        playerInput = new PlayerInputActions.PlayerInputActions();
        playerInput.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        playerInput.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerInput.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        playerInput.Player.Jump.performed += ctx => Jump();

        playerInput.Player.Sprint.started += ctx => StartSprint();
        playerInput.Player.Sprint.canceled += ctx => StopSprint();

        playerInput.Player.Crouch.performed += ctx => ToggleCrouch();
    }

    private void OnEnable() => playerInput.Enable();
    private void OnDisable() => playerInput.Disable();

    private void Update()
    {
        Move();
        Look();
        UpdateCamera();
    }

    private void Move()
    {
        //Set speed based on player state.
        currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        bool grounded = controller.isGrounded;
        if (grounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        // Handle landing bob effect.
        if (grounded && !wasGrounded && !landingTriggered)
        {
            landingBobOffset = landingBobIntensity;
            landingTriggered = true;
        }
        else if (!grounded)
        {
            landingTriggered = false;
        }

        wasGrounded = grounded;
    }

    private void Look()
    {
        float yaw = lookInput.x * lookSensitivity;
        transform.Rotate(Vector3.up, yaw);

        float pitch = lookInput.y * lookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch - pitch, -90f, 90f);
    }

    private void UpdateCamera()
    {
        if (playerCamera == null)
            return;

        //Apply camera pitch and head bob effects.
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        float bobOffsetY = 0f;
        if (moveInput.sqrMagnitude > 0.01f && controller.isGrounded)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            bobOffsetY = Mathf.Sin(bobTimer) * bobAmount;
        }
        else
        {
            bobTimer = 0f;
        }

        //Smoothly reduce landing bob effect.
        landingBobOffset = Mathf.Lerp(landingBobOffset, 0f, Time.deltaTime * landingBobDecaySpeed);

        //Adjust camera position based on crouching state and bob effects.
        Vector3 targetPos = defaultCameraLocalPos;
        if (isCrouching)
            targetPos.y -= crouchCameraOffset;

        targetPos.y += bobOffsetY + landingBobOffset;
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetPos, Time.deltaTime * 10f);
    }

    private void Jump()
    {
        if (controller.isGrounded)
            verticalVelocity = jumpForce;
    }

    private void StartSprint() => isSprinting = true;
    private void StopSprint() => isSprinting = false;

    private void ToggleCrouch()
    {
        if (isCrouching)
        {
            //Stand up if there's enough room.
            if (CanStand())
            {
                isCrouching = false;
                controller.height = defaultControllerHeight;
                controller.center = defaultControllerCenter;
            }
        }
        else
        {
            isCrouching = true;
            controller.height = defaultControllerHeight - crouchHeightReduction;
            controller.center = defaultControllerCenter + new Vector3(0, -crouchHeightReduction / 2f, 0);
        }
    }

    private bool CanStand()
    {
        Vector3 bottom = transform.position + Vector3.up * controller.radius;
        Vector3 top = transform.position + Vector3.up * (defaultControllerHeight - controller.radius);
        return !Physics.CheckCapsule(bottom, top, controller.radius);
    }
}
