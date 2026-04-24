using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    
    public float smoothTime = 0.25f;
    
    public Vector3 offset = new Vector3(0f, 0f, 0f);

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        Vector3 pos = transform.position;
        pos.x = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime).x;
        transform.position = pos;
    }
}
