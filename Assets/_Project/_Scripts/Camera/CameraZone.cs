using UnityEngine;
using Unity.Cinemachine;

public class CameraZone : MonoBehaviour
{
    [Header("Cinemachine v3 Setup")]
    [SerializeField] private CinemachineCamera zoneCamera;
    
    // Static means this variable is shared across ALL CameraZone scripts in the game.
    private static int globalPriorityCounter = 10; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.name.Contains("Arthur"))
        {
            if (zoneCamera != null)
            {
                // Increase the global highest number, then assign it to this camera
                globalPriorityCounter++;
                zoneCamera.Priority = globalPriorityCounter;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.name.Contains("Arthur"))
        {
            if (zoneCamera != null)
            {
                // Reset this specific camera back to 0 when leaving
                zoneCamera.Priority = 0;
            }
        }
    }
}