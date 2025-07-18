using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("Assign the Camera responsible for viewing the Player's side (Right half).")]
    public Camera playerCamera;

    [Tooltip("Assign the Camera responsible for viewing the Ghost Player's side (Left half).")]
    public Camera ghostCamera;

    [Header("Player References")]
    [Tooltip("Assign the PlayerController for the main player.")]
    public PlayerController playerController;

    [Tooltip("Assign the GhostPlayer for the ghost character.")]
    public GhostPlayer ghostPlayer;

    [Header("Camera Settings")]
    [Tooltip("The offset from the player's position where the camera should be placed.")]
    public Vector3 cameraOffset = new Vector3(0f, 5f, -10f); // Example offset: 5 units up, 10 units back

    [Tooltip("The speed at which the camera interpolates towards its target position.")]
    [Range(1f, 20f)] // A reasonable range for interpolation speed
    public float interpolationSpeed = 5f; // New variable for Lerp speed

    void Awake()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera is not assigned in CameraSplitter. Please assign it in the Inspector.");
            return;
        }
        if (ghostCamera == null)
        {
            Debug.LogError("Ghost Camera is not assigned in CameraSplitter. Please assign it in the Inspector.");
            return;
        }
        if (playerController == null)
        {
            Debug.LogError("Player Controller is not assigned in CameraSplitter. Please assign it in the Inspector.");
            return;
        }
        if (ghostPlayer == null)
        {
            Debug.LogError("Ghost Player is not assigned in CameraSplitter. Please assign it in the Inspector.");
            return;
        }

    }

    void LateUpdate()
    {
        // Only update camera positions if the game is running
        if (GameManager.Instance != null && GameManager.Instance.IsGameRunning)
        {
            FollowPlayers();
        }
    }

    private void FollowPlayers()
    {
        // Player Camera follows the main player
        if (playerCamera != null && playerController != null)
        {
            Vector3 targetPlayerPosition = playerController.transform.position + cameraOffset;
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPlayerPosition, interpolationSpeed * Time.deltaTime);
        }

        // Ghost Camera follows the ghost player
        if (ghostCamera != null && ghostPlayer != null)
        {
            Vector3 targetGhostPosition = ghostPlayer.transform.position + cameraOffset;
            ghostCamera.transform.position = Vector3.Lerp(ghostCamera.transform.position, targetGhostPosition, interpolationSpeed * Time.deltaTime);
        }
    }
}
