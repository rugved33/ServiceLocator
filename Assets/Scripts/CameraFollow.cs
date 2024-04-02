using System.Linq;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    private float _PositionSmoothTime = 0.2f;
    private float _RotationSmoothTime = 0.2f;
    private float _MaxZoomOutMultiplier = 0.1f;
    private float _ZoomSpeed = 5f;
    private float _InitialOrthoSize;
    private float _MaxOrthoSize;
    private float _VerticalOffset = -1.0f;

    private Transform _Target;
    private Transform _CameraTransform;
    private Camera _MainCamera;

    private Vector3 m_Velocity = Vector3.zero;

    private void Start()
    {

        _CameraTransform = transform;
        _MainCamera = GetComponent<Camera>();
        _MaxOrthoSize = _InitialOrthoSize * (1f + _MaxZoomOutMultiplier);
    }

    private void LateUpdate()
    {
        if (_Target == null)
            return;

        var position = _Target.position;
        var transformPosition = _CameraTransform.position;

        var isTouching = Input.touchCount > 0;

        var adjustedYOffset = _VerticalOffset + 0.5f * _MainCamera.orthographicSize;
        var rotatedOffset = Quaternion.Euler(0, 0, _Target.eulerAngles.z) * new Vector3(0, adjustedYOffset, 0);
        var clampedX = position.x + rotatedOffset.x;
        var clampedY = position.y + rotatedOffset.y;

        var desiredPosition = new Vector3(clampedX, clampedY, transformPosition.z);
        var smoothedPosition = Vector3.SmoothDamp(transformPosition, desiredPosition, ref m_Velocity, _PositionSmoothTime);
        _CameraTransform.position = smoothedPosition;

        var targetOrthographicSize = isTouching
            ? Mathf.Clamp(_MainCamera.orthographicSize * (1f + _MaxZoomOutMultiplier), _InitialOrthoSize, _MaxOrthoSize)
            : _InitialOrthoSize;

        _MainCamera.orthographicSize = Mathf.Lerp(_MainCamera.orthographicSize, targetOrthographicSize, Time.deltaTime * _ZoomSpeed);

        var desiredRotation = Quaternion.LookRotation(_Target.forward, _Target.up);
        _CameraTransform.rotation = Quaternion.Slerp(_CameraTransform.rotation, desiredRotation, _RotationSmoothTime);
    }

    public void SetTarget(Transform trans)
    {
        _Target = trans;
    }

    public void ClearTarget()
    {
        _Target = null;
    }
}