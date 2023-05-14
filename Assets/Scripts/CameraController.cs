using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target; // referensi ke karakter yang akan diikuti
    [SerializeField] private float smoothSpeed = 0.125f; // kecepatan pergerakan kamera

    private Vector3 offset; // jarak antara kamera dan karakter

    void Start()
    {
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
