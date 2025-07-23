using System.Collections;
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
    public Vector3 cameraOffset = new Vector3(0f, 5f, -10f); 
    [Tooltip("The speed at which the camera interpolates towards its target position.")]
    [Range(1f, 20f)] 
    public float interpolationSpeed = 5f;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Advanced Settings")]
    [SerializeField] private bool useRandomDirection = true;
    [SerializeField] private Vector3 shakeDirection = Vector3.one;
    [SerializeField] private float rotationIntensity = 0f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Coroutine shakeCoroutine;
    private bool isShaking = false;

    public Transform target;
    public float speed = 90f;  
    public Vector3 axis = Vector3.up;

    public static CameraManager instance;





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
        if(instance == null)
        {
            instance = this;
        }

    }

    private void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    public void StartRotating(Transform orbit)
    {
        target = orbit;
        StartCoroutine(RotateCoroutine());
    }

    IEnumerator RotateCoroutine()
    {
        while (true)               
        {
            playerCamera.transform.RotateAround(target.position, axis, speed * Time.deltaTime);
            yield return null;    
        }
    }

    void LateUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameRunning)
        {
            FollowPlayers();
        }
    }

    private void FollowPlayers()
    {
        if (playerCamera != null && playerController != null)
        {
            Vector3 targetPlayerPosition = playerController.transform.position + cameraOffset;
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPlayerPosition, interpolationSpeed * Time.deltaTime);
        }

        if (ghostCamera != null && ghostPlayer != null)
        {
            Vector3 targetGhostPosition = ghostPlayer.transform.position + cameraOffset;
            ghostCamera.transform.position = Vector3.Lerp(ghostCamera.transform.position, targetGhostPosition, interpolationSpeed * Time.deltaTime);
        }
    }




    public void Shake()
    {
        Shake(shakeDuration, shakeIntensity);
    }


    public void Shake(float duration, float intensity)
    {
        Shake(duration, intensity, rotationIntensity);
    }

    public void Shake(float duration, float intensity, float rotIntensity)
    {
        if (isShaking && shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity, rotIntensity));
    }


    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isShaking = false;
    }

    private IEnumerator ShakeCoroutine(float duration, float intensity, float rotIntensity)
    {
        isShaking = true;
        float elapsed = 0f;
        //Debug.Log("ShakeCoroutine just ran");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            float currentIntensity = intensity * shakeCurve.Evaluate(progress);
            float currentRotIntensity = rotIntensity * shakeCurve.Evaluate(progress);

            Vector3 shakeOffset;
            if (useRandomDirection)
            {
                shakeOffset = Random.insideUnitSphere * currentIntensity;
            }
            else
            {
                shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * shakeDirection.x,
                    Random.Range(-1f, 1f) * shakeDirection.y,
                    Random.Range(-1f, 1f) * shakeDirection.z
                ) * currentIntensity;
            }

            transform.localPosition = originalPosition + shakeOffset;

            if (currentRotIntensity > 0)
            {
                Vector3 rotationShake = new Vector3(
                    Random.Range(-1f, 1f) * currentRotIntensity,
                    Random.Range(-1f, 1f) * currentRotIntensity,
                    Random.Range(-1f, 1f) * currentRotIntensity
                );

                transform.localRotation = originalRotation * Quaternion.Euler(rotationShake);
            }

            yield return null;
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isShaking = false;
    }

    public void UpdateOriginalPosition()
    {
        if (!isShaking)
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }
    }

    public bool IsShaking => isShaking;

    public void ExplosionShake()
    {
        Shake(0.8f, 0.3f, 2f);
    }

    public void HitShake()
    {

        Shake(0.2f, 0.15f, 1f);
    }

    public void EarthquakeShake()
    {
        Shake(2f, 0.2f, 0.5f);
    }

    public void SubtleShake()
    {
        Shake(0.3f, 0.05f, 0f);
    }


}
