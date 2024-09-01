using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 _offset = new Vector3(-8.41f, 13.26f, -7.31f);
    public Transform target; // The jet the camera will follow
    [SerializeField] private float smoothTime = 7f;
    private Vector3 _currentVelocity = Vector3.zero;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + _offset;   
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            transform.position = target.position + _offset;
        }
    }

    public bool IsTargetAlive()
    {
        return target != null;
    }
}
