using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControls : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera _camera;
    [SerializeField]
    private float _speedHorizontal;
    [SerializeField] 
    private float _speedVertical;
    [SerializeField, Tooltip("This is in radians"), Range(-3.14f, 0)]
    private float _minVerticalRotation = -30;
    [SerializeField, Tooltip("This is in radians"), Range(0,3.14f)] 
    private float _maxVerticalRotation = 30;
    [SerializeField]
    private InputActionReference _lookInput;

    private Vector2 _mouseInput;

    private float _verticalRotation;
    private Vector2 _originalCameraOffset;
    private float _cameraDistance;

    private CinemachineTransposer _camTransposer;
    private CinemachineComposer _camComposer;
    private Vector3 _lookPoint;

    private bool _lockedOn;
    private Transform _target;

    private void Start()
    {
        _camTransposer = _camera.GetCinemachineComponent<CinemachineTransposer>();
        _cameraDistance = _camTransposer.m_FollowOffset.magnitude;
        _originalCameraOffset = new Vector2(_camTransposer.m_FollowOffset.y, _camTransposer.m_FollowOffset.z);
        _camComposer = _camera.GetCinemachineComponent<CinemachineComposer>();
        _lookPoint = _camComposer.m_TrackedObjectOffset;
        StartCoroutine(DoCameraLogic());
    }

    private IEnumerator DoCameraLogic()
    {
        while (true) 
        {
            _mouseInput = _lookInput.action.ReadValue<Vector2>();
            if (_mouseInput.sqrMagnitude < 0.1)
            {
                yield return null;
                continue;
            }

            if (_lockedOn && _target != null)
            {
                Vector3 newOffset = (_target.position - transform.position + _lookPoint) / 2;
                _camComposer.m_TrackedObjectOffset = newOffset;
            }
            else
            {
                float horizontalRotation = _mouseInput.x * _speedHorizontal * Time.deltaTime;
                transform.Rotate(Vector3.up, horizontalRotation);

                _verticalRotation += _mouseInput.y * _speedVertical * Time.deltaTime;

                _verticalRotation = Mathf.Clamp(_verticalRotation, _minVerticalRotation, _maxVerticalRotation);

                Vector2 newRotation = RotationMatrix(_verticalRotation, _originalCameraOffset);
                Vector3 newOffset = new Vector3(0, newRotation.x, newRotation.y);
                _camTransposer.m_FollowOffset = newOffset;
            }

            yield return null;
        }
    }

    private Vector2 RotationMatrix(float theta, Vector2 vector)
    {
        float sin = MathF.Sin(theta);
        float cos = MathF.Cos(theta);

        Vector2 row1 = new Vector2(cos, -sin);
        Vector2 row2 = new Vector2(sin, cos);

        float x = row1.x * vector.x + row1.y * vector.y;
        float y = row2.x * vector.x + row2.y * vector.y;

        return new Vector2(x, y);
    }

    public void SetLockOnTarget(Component sender, object data)
    {
        Transform target = data as Transform;

        if (target == null)
        {
            _lockedOn = false;
            _camComposer.m_TrackedObjectOffset = _lookPoint;
            _camTransposer.m_FollowOffset = _originalCameraOffset;
            return;
        }
        _lockedOn = true;
        _target = target;
        _camTransposer.m_FollowOffset = _originalCameraOffset;
    }
}
