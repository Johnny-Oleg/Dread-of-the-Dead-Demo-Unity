using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.Animations.Rigging; // 1. Added the Animation Rigging namespace

public class PlayerAiming : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private CinemachineCamera otsAimCamera;
    [SerializeField] private GameObject reticleUI;
    [SerializeField] private Transform camTarget;

    [Header("Rig Settings")]
    [SerializeField] private Rig aimRig;              // 2. Drag the 'Rig_Setup' object here
    [SerializeField] private float rigBlendSpeed = 8f; // How fast Arthur transitions into/out of his aim pose


    [Header("Aiming Stats")]
    [SerializeField] private float aimTurnSpeed = 150.0f; 
    [SerializeField] private float aimPitchSpeed = 150.0f; 
    [SerializeField] private float minAimPitch = -45f;     
    [SerializeField] private float maxAimPitch = 45f;      

    // Public auto-property that other scripts can read, but only this script can change
    public bool IsAiming { get; private set; } = false;

    private float currentAimPitch = 0f; 
    private PlayerInputControls inputActions;
    private bool debugAimToggle = false;

    private void Awake()
    {
        inputActions = new PlayerInputControls();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        
        if (reticleUI != null) reticleUI.SetActive(false);
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        HandleInputStates();
        BlendRigWeight(); // 3. Run the smooth weight blending engine every frame
        
        if (IsAiming) 
        {
            HandleAimingRotation();
        }
    }

    private void HandleInputStates()
    {
        if (Keyboard.current.rightShiftKey.wasPressedThisFrame) debugAimToggle = !debugAimToggle;

        IsAiming = inputActions.Player.Aim.IsPressed() || debugAimToggle;

        // Reset camera pitch smoothly back to horizon center when dropping aim
        if (!IsAiming) currentAimPitch = Mathf.Lerp(currentAimPitch, 0f, Time.deltaTime * 5f);
        
        if (otsAimCamera != null) otsAimCamera.Priority = IsAiming ? 100 : 0;
        if (reticleUI != null) reticleUI.SetActive(IsAiming);

        animator.SetBool("IsAiming", IsAiming);
    }

    // 4. Smoothly slides the Rig weight between 0 (Normal) and 1 (Aiming)
    private void BlendRigWeight()
    {
        if (aimRig == null) return;

        float targetWeight = IsAiming ? 1f : 0f;
        aimRig.weight = Mathf.Lerp(aimRig.weight, targetWeight, Time.deltaTime * rigBlendSpeed);
    }

    private void HandleAimingRotation()
    {
        // Force movement speed animations to stop while free-aiming
        animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);

        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        // 1. Horizontal (Yaw) - Turns Arthur's entire body capsule left/right
        float yawRotation = lookInput.x * aimTurnSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, yawRotation);

        // 2. Vertical (Pitch) - Tilts the IK Anchor up/down
        if (camTarget != null)
        {
            currentAimPitch -= lookInput.y * aimPitchSpeed * Time.deltaTime;
            currentAimPitch = Mathf.Clamp(currentAimPitch, minAimPitch, maxAimPitch);
            
            camTarget.localEulerAngles = new Vector3(currentAimPitch, 0f, 0f);
        }
    }
}