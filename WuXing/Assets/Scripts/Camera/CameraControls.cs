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
    private float _minVerticalRotation = -0.5236f;
    [SerializeField, Tooltip("This is in radians"), Range(0,3.14f)] 
    private float _maxVerticalRotation = 0.5236f;
    [SerializeField]
    private InputActionReference _lookInput;
    [SerializeField]
    private Transform _lookAtTarget;

    private Vector2 _mouseInput;

    private float _verticalRotation;
    private Vector2 _originalCameraOffset;
    private float _cameraDistance;

    private CinemachineTransposer _camTransposer;
    private CinemachineComposer _camComposer;
    private Vector3 _lookPoint;

    [SerializeField]
    private LockOn _lockOn;
    private bool _lockedOn;
    private Transform _target;
    private Transform _originTarget;

    private void Start()
    {
        _camTransposer = _camera.GetCinemachineComponent<CinemachineTransposer>();
        _cameraDistance = _camTransposer.m_FollowOffset.magnitude;
        _originalCameraOffset = new Vector2(_camTransposer.m_FollowOffset.y, _camTransposer.m_FollowOffset.z);
        _camComposer = _camera.GetCinemachineComponent<CinemachineComposer>();
        _lookPoint = _camComposer.m_TrackedObjectOffset;
        _originTarget = _camera.LookAt;
        _lockOn.LockOnStart += LockOnStart;
        _lockOn.LockOnStop += LockOnStop;
        StartCoroutine(DoCameraLogic());
    }

    private IEnumerator DoCameraLogic()
    {
        while (true) 
        {
            Vector3 newOffset;
            if (_lockedOn && _target != null)
            {
                newOffset = (_target.position - transform.position + _lookPoint) / 2;
                _lookAtTarget.position = newOffset + _originTarget.position;

                Vector3 directionToTarget = _lookAtTarget.position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _speedHorizontal);

                yield return null;
                continue;
            }
            if (_lockedOn && _target == null)
                LockOnStop(this, EventArgs.Empty);

            _mouseInput = _lookInput.action.ReadValue<Vector2>();
            if (_mouseInput.sqrMagnitude < 0.1)
            {
                yield return null;
                continue;
            }

            float horizontalRotation = _mouseInput.x * _speedHorizontal * Time.deltaTime;
            transform.Rotate(Vector3.up, horizontalRotation);

            _verticalRotation += _mouseInput.y * _speedVertical * Time.deltaTime;

            _verticalRotation = Mathf.Clamp(_verticalRotation, _minVerticalRotation, _maxVerticalRotation);

            Vector2 newRotation = RotationMatrix(_verticalRotation, _originalCameraOffset);
            newOffset = new Vector3(0, newRotation.x, newRotation.y);
            _camTransposer.m_FollowOffset = newOffset;
            

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

    private void LockOnStop(object sender, EventArgs e)
    {
        _target = null;
        _lockedOn = false;
        _camera.LookAt = _originTarget;
        _camComposer.m_TrackedObjectOffset = _lookPoint;
        _camTransposer.m_FollowOffset = new Vector3(0, _originalCameraOffset.x, _originalCameraOffset.y);
    }

    private void LockOnStart(object sender, Transform e)
    {
        if (e == null) return;

        _lockedOn = true;
        _target = e;
        _camera.LookAt = _lookAtTarget;
        _camComposer.m_TrackedObjectOffset = Vector3.zero;
        _camTransposer.m_FollowOffset = new Vector3(0, _originalCameraOffset.x, _originalCameraOffset.y);
    }
}
