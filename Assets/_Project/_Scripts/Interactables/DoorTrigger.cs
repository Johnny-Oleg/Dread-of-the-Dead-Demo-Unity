using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("Door Leaves (Hinge Objects)")]
    [SerializeField] private Transform leftDoorHinge;
    [SerializeField] private Transform rightDoorHinge; // Leave unassigned for single doors

    [Header("Swing Configuration")]
    [SerializeField] private float openAngleLeft = -90f;
    [SerializeField] private float openAngleRight = 90f;
    [SerializeField] private float openSpeed = 3.5f;

    private Quaternion leftClosedRot;
    private Quaternion rightClosedRot;
    private Quaternion leftOpenRot;
    private Quaternion rightOpenRot;

    private bool isOpen = false;

    private void Start()
    {
        if (leftDoorHinge != null)
        {
            leftClosedRot = leftDoorHinge.localRotation;
            leftOpenRot = leftClosedRot * Quaternion.Euler(0f, openAngleLeft, 0f);
        }

        if (rightDoorHinge != null)
        {
            rightClosedRot = rightDoorHinge.localRotation;
            rightOpenRot = rightClosedRot * Quaternion.Euler(0f, openAngleRight, 0f);
        }
    }

    private void Update()
    {
        Quaternion targetLeft = isOpen ? leftOpenRot : leftClosedRot;
        Quaternion targetRight = isOpen ? rightOpenRot : rightClosedRot;

        if (leftDoorHinge != null)
        {
            leftDoorHinge.localRotation = Quaternion.Slerp(leftDoorHinge.localRotation, targetLeft, Time.deltaTime * openSpeed);
        }

        if (rightDoorHinge != null)
        {
            rightDoorHinge.localRotation = Quaternion.Slerp(rightDoorHinge.localRotation, targetRight, Time.deltaTime * openSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.name.Contains("Arthur"))
        {
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.name.Contains("Arthur"))
        {
            isOpen = false;
        }
    }
}