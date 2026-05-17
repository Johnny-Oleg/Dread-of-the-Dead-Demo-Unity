using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform mainCameraTransform;

    [Header("Movement Stats")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 6.0f;
    [SerializeField] private float rotationSpeed = 10.0f;
    [SerializeField] private float gravity = -9.81f;

    private PlayerInputControls inputActions;
    private Vector3 velocity; // Used for gravity
    
    // Tracking states
    private bool isAiming = false;

    private void Awake()
    {
        inputActions = new PlayerInputControls();
        controller = GetComponent<CharacterController>();
        
        // Automatically find the main camera if we forgot to drag it in
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
        
        // If we are aiming, GDD says we CANNOT move.
        if (!isAiming)
        {
            HandleMovement();
        }
        
        ApplyGravity();
    }

    private void HandleInputStates()
    {
        // Check if aiming button is held down
        isAiming = inputActions.Player.Aim.IsPressed();
    }

    private void HandleMovement()
    {
        // 1. Get raw input
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        bool isRunning = inputActions.Player.Run.IsPressed();

        // 2. Calculate camera-relative directions
        Vector3 camForward = mainCameraTransform.forward;
        Vector3 camRight = mainCameraTransform.right;

        // Flatten the vectors so Arthur doesn't tilt up/down
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 3. Create the final movement direction
        Vector3 moveDirection = (camForward * input.y + camRight * input.x).normalized;

        // 4. Move the character
        if (moveDirection.magnitude >= 0.1f)
        {
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            // 5. Rotate Arthur to face where he is walking
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void ApplyGravity()
    {
        // Basic gravity logic so Arthur stays on the floor
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small constant downward force to stick to slopes
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}

