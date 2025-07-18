using UnityEngine;

public class Rotation : MonoBehaviour
{
    [Tooltip("Speed of rotation in degrees per second.")]
    public float rotationSpeed = 90f; // Default rotation speed

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
