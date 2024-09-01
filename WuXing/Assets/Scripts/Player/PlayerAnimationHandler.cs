using System.Collections;
using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private Animator _animator;

    private static readonly int Falling = Animator.StringToHash("Falling");
    private static readonly int CastSpell = Animator.StringToHash("CastSpell");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int StrafeLeft = Animator.StringToHash("StrafeLeft");
    private static readonly int StrafeRight = Animator.StringToHash("StrafeRight");
    private static readonly int RunBack = Animator.StringToHash("RunBack");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void UpdateMovementAnimations(Directions moveDirection)
    {
        // Reset all movement-related animation bools
        SetAnimationBool(Run, false);
        SetAnimationBool(RunBack, false);
        SetAnimationBool(StrafeLeft, false);
        SetAnimationBool(StrafeRight, false);

        // Set appropriate animation bool based on the direction
        if (moveDirection.HasFlag(Directions.Front))
        {
            SetAnimationBool(Run, true);
        }
        else if (moveDirection.HasFlag(Directions.Back))
        {
            SetAnimationBool(RunBack, true);
        }

        if (moveDirection.HasFlag(Directions.Left))
        {
            SetAnimationBool(StrafeLeft, true);
        }
        else if (moveDirection.HasFlag(Directions.Right))
        {
            SetAnimationBool(StrafeRight, true);
        }
    }

    public void StopMoving()
    {
        SetAnimationBool(Run, false);
        SetAnimationBool(RunBack, false);
        SetAnimationBool(StrafeLeft, false);
        SetAnimationBool(StrafeRight, false);
    }

    public void UpdateFallingAnimation(bool isFalling)
    {
        SetAnimationBool(Falling, isFalling);
    }

    public void TriggerCastSpell()
    {
        SetAnimationBoolForDuration(CastSpell, true, 0.5f); // Example duration, adjust based on your animation
    }

    private void SetAnimationBool(int hash, bool value)
    {
        _animator.SetBool(hash, value);
    }

    private void SetAnimationBoolForDuration(int hash, bool newState, float duration)
    {
        StartCoroutine(SetAnimationBoolCoroutine(hash, newState, duration));
    }

    private IEnumerator SetAnimationBoolCoroutine(int hash, bool newState, float duration)
    {
        SetAnimationBool(hash, newState);
        yield return new WaitForSeconds(duration);
        SetAnimationBool(hash, !newState);
    }
}
