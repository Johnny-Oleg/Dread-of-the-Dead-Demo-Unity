using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private Animator animator;

    [Header("Movement Stats")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 10.0f;
    [SerializeField] private float gravity = -9.81f;

    private PlayerInputControls inputActions;
    private Vector3 velocity; 
    private bool isAiming = false;

    private void Awake()
    {
        inputActions = new PlayerInputControls();
        controller = GetComponent<CharacterController>();
        
        if (animator == null) animator = GetComponentInChildren<Animator>();
        
        if (mainCameraTransform == null && Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
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
            // If aiming, force speed to 0 so he stops walking
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }
        
        ApplyGravity();
    }

    private void HandleInputStates()
    {
        isAiming = inputActions.Player.Aim.IsPressed();
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

        // Calculate our current movement magnitude (0 to 1)
        float inputMagnitude = moveDirection.magnitude;

        if (inputMagnitude >= 0.1f)
        {
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Send speed to the Animator (normalized between 0 and 1)
            float animationSpeedPercent = isRunning ? 1f : 0.5f; 
            animator.SetFloat("Speed", animationSpeedPercent, 0.1f, Time.deltaTime); // 0.1f adds smoothing
        }
        else
        {
            // Tell animator to go to Idle
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
