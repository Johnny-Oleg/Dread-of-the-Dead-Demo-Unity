using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerAiming))] // Ensures aiming script is always attached
public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private Animator animator;
    private PlayerAiming playerAiming;

    [Header("Movement Stats")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 5.0f;
    [SerializeField] private float rotationSpeed = 10.0f;
    [SerializeField] private float gravity = -9.81f;

    private PlayerInputControls inputActions;
    private Vector3 velocity; 

    private void Awake()
    {
        inputActions = new PlayerInputControls();
        controller = GetComponent<CharacterController>();
        playerAiming = GetComponent<PlayerAiming>();
        
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (mainCameraTransform == null && Camera.main != null) mainCameraTransform = Camera.main.transform;
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        // Only allow free locomotion if Arthur isn't actively aiming
        if (!playerAiming.IsAiming)
        {
            HandleMovement();
        }
        
        ApplyGravity();
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

            // Rotate Arthur to face his moving direction
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