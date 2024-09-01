using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerAnimationHandler))]
public class Movement : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10f;
    [SerializeField]
    private float _dashSpeedMulti = 2f;
    [SerializeField]
    private float _dashTime = 1f;
    [SerializeField]
    private InputActionReference _input;
    [SerializeField]
    private float _rotationSpeed = 0.1f;

    [SerializeField]
    private float _gravityMultiplier = 1.2f;

    private const float _gravity = -9.81f;
    private float _velocityY = 0f;
    private float _fallTime = 0f;
    private bool _fallingValue = false;
    private bool _falling
    {
        get { return _fallingValue; }
        set
        {
            if (value != _fallingValue)
            {
                _fallingValue = value;
                _animator.UpdateFallingAnimation(value);

                if (value)
                {
                    StartFalling();
                }
            }
        }
    }

    private Vector3 _movement = Vector3.zero;

    private Transform _target;
    private bool _lockedOn = false;

    private bool _dashing;

    private CharacterController _controller;
    private Transform _cam;
    private PlayerAnimationHandler _animator;
    private LockOn _lockOn;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<PlayerAnimationHandler>();
        _cam = Camera.main.transform;
        _lockOn = GetComponent<LockOn>();
        _lockOn.LockOnStart += LockOnStart;
        _lockOn.LockOnStop += LockOnStop;
    }

    private void Update()
    {
        GetHorizontalInput();
        bool isMoving = _movement.magnitude >= 0.1f;

        if (isMoving)
            RotateTowardsMovement();
        else
            _animator.StopMoving();        

        ApplyGravity();
        ExecuteMovement();
    }

    private void GetHorizontalInput()
    {
        if (_dashing) 
            return;

        Vector2 inputDirection = _input.action.ReadValue<Vector2>();

        Vector3 cameraForward = _cam.forward;
        cameraForward.y = 0;
        cameraForward = cameraForward.normalized;

        Vector3 cameraSidewards = Vector3.Cross(cameraForward, Vector3.up).normalized;

        float horizontalInput = -inputDirection.x;
        float verticalInput = inputDirection.y;

        _movement += (cameraForward * verticalInput + cameraSidewards * horizontalInput) * _speed;
    }

    private void RotateTowardsMovement()
    {
        Directions moveDirection = GetClosestDirection(_movement.normalized);
        _animator.UpdateMovementAnimations(moveDirection);

        Quaternion targetRotation;
        if (_lockedOn && _target != null)
        {
            // Calculate the direction to the target
            Vector3 directionToTarget = (_target.position - transform.position).normalized;
            directionToTarget.y = 0f; // Keep the rotation on the horizontal plane

            // Calculate the target rotation based on the direction to the target
            targetRotation = Quaternion.LookRotation(directionToTarget);

            // Smoothly rotate towards the target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            return;
        }

        targetRotation = Quaternion.LookRotation(_movement.normalized, Vector3.up);

        // Smoothly interpolate between the current rotation and the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded)
        {
            if (_velocityY < 0f) _velocityY = -1f;
            _falling = false;
        }
        else
        {
            _falling = true;
            float gravityEffect = _gravity * _gravityMultiplier;
            _velocityY += Mathf.Min(-1, gravityEffect * _fallTime * _fallTime);
        }

        _movement.y = _velocityY;
    }

    private void ExecuteMovement()
    {
        _controller.Move(_movement * Time.deltaTime);
        _movement = Vector3.zero;
    }

    private void StartFalling()
    {
        // _animator.SetAnimationBool(PlayerAnimationBools.Falling, true);
        StartCoroutine(FallingTime());
    }

    private IEnumerator FallingTime()
    {
        _fallTime = 0;

        while (_falling)
        {
            _fallTime += Time.deltaTime;
            yield return null;
        }

        _fallTime = 0f;
    }

    public float GetYVelocity()
    {
        return _velocityY;
    }

    public Vector3 GetVelocity()
    {
        return _controller.velocity;
    }

    public void StartDash(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || _dashing)
            return;

        StartCoroutine(StartDashing());
    }

    private IEnumerator StartDashing()
    {
        _dashing = true;

        Vector2 inputDirection = _input.action.ReadValue<Vector2>();

        Vector3 cameraForward = _cam.forward;
        cameraForward.y = 0;
        cameraForward = cameraForward.normalized;

        Vector3 cameraSidewards = Vector3.Cross(cameraForward, Vector3.up).normalized;

        float horizontalInput = -inputDirection.x;
        float verticalInput = inputDirection.y;

        Vector3 direction = _speed * _dashSpeedMulti * (cameraForward * verticalInput + cameraSidewards * horizontalInput).normalized;

        float time = 0;
        while (time < _dashTime)
        {
            _controller.Move(direction * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;
        }

        _dashing = false;
    }

    private Directions GetClosestDirection(Vector3 direction)
    {
        Directions closestDirection = Directions.None;

        Vector3 checkVector = direction;

        float closeness = -1f;

        for (int i = 0; i < 4; i++)
        {
            Vector3 globalVector = GetDirectionVector(i);
            float newCloseness = Vector3.Dot(globalVector, checkVector);
            if (newCloseness > closeness)
            {
                closeness = newCloseness;
                closestDirection = (Directions)(1 << i);
            }

        }

        return closestDirection;
    }

    private Vector3 GetDirectionVector(int directionIndex)
    {
        // The order of these directions are important, as they match up with the order in the Direction Enum
        switch (directionIndex)
        {
            case 0:
                return transform.forward;
            case 1:
                return -transform.forward;
            case 2:
                return -transform.right;
            case 3:
                return transform.right;
            default:
                return Vector3.zero;
        }
    }

    private void LockOnStop(object sender, EventArgs e)
    {
        _target = null;
        _lockedOn = false;
    }

    private void LockOnStart(object sender, Transform e)
    {
        if (e == null) return;

        _lockedOn = true;
        _target = e;
    }
}
