using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private CinemachineCamera otsAimCamera;
    [SerializeField] private GameObject reticleUI;
    [SerializeField] private Transform camTarget;


    [Header("Movement Stats")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 5.0f;
    [SerializeField] private float rotationSpeed = 10.0f;
    [SerializeField] private float gravity = -9.81f;


    [Header("Aiming Stats")]
    [SerializeField] private float aimTurnSpeed = 150.0f; 
    [SerializeField] private float aimPitchSpeed = 150.0f; // How fast Arthur turns while aiming
    [SerializeField] private float minAimPitch = -45f;     // Max Look Up angle
    [SerializeField] private float maxAimPitch = 45f;      // Max Look Down angle
    
    private float currentAimPitch = 0f; // <-- Tracks our current vertical angle
    private PlayerInputControls inputActions;
    private Vector3 velocity; 
    
    private bool isAiming = false;
    private bool debugAimToggle = false;

    private void Awake()
    {
        inputActions = new PlayerInputControls();
        controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (mainCameraTransform == null && Camera.main != null) mainCameraTransform = Camera.main.transform;
        
        // Hide the reticle on start
        if (reticleUI != null) reticleUI.SetActive(false);
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        HandleInputStates();
        
        if (!isAiming)
        {
            HandleMovement();
        }
        else 
        {
            HandleAimingRotation();
        }
        
        ApplyGravity();
    }

    private void HandleInputStates()
    {
        if (Keyboard.current.rightShiftKey.wasPressedThisFrame) debugAimToggle = !debugAimToggle;

        isAiming = inputActions.Player.Aim.IsPressed() || debugAimToggle;

        // If we are NOT aiming, gently reset the camera pitch back to center (0)
        if (!isAiming) currentAimPitch = Mathf.Lerp(currentAimPitch, 0f, Time.deltaTime * 5f);
        
        if (otsAimCamera != null) otsAimCamera.Priority = isAiming ? 100 : 0;

        // Toggle the UI dot based on aiming state
        if (reticleUI != null) reticleUI.SetActive(isAiming);

        animator.SetBool("IsAiming", isAiming);
    }

    // Allows rotating Arthur horizontally while aiming
    private void HandleAimingRotation()
    {
        // Stop the walking animation
        animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);

        // Read the mouse delta or right analog stick
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        // 1. Horizontal (Yaw) - Rotates Arthur's Entire Body Left/Right
        float yawRotation = lookInput.x * aimTurnSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, yawRotation);

        // 2. Vertical (Pitch) - Tilts the Camera Target Up/Down
        if (camTarget != null)
        {
            // Note: We SUBTRACT here because standard mouse-Y is inverted in Unity's rotation math.
            // If a player's aiming feels inverted (up looks down), change this to +=
            currentAimPitch -= lookInput.y * aimPitchSpeed * Time.deltaTime;
            
            // Clamp the angle so the camera doesn't flip upside down
            currentAimPitch = Mathf.Clamp(currentAimPitch, minAimPitch, maxAimPitch);
            
            // Apply the local rotation to the target anchor
            camTarget.localEulerAngles = new Vector3(currentAimPitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        bool isRunning = inputActions.Player.Run.IsPressed();

        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * input.y + camRight * input.x).normalized;
        float inputMagnitude = moveDirection.magnitude;

        if (inputMagnitude >= 0.1f)
        {
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            float animationSpeedPercent = isRunning ? 1f : 0.5f; 
            animator.SetFloat("Speed", animationSpeedPercent, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f; 
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}