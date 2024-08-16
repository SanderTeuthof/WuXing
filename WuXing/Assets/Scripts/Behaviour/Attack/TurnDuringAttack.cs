using System.Collections;
using System;
using UnityEngine;

public class TurnDuringAttack : MonoBehaviour, INPCAttackState
{
    [SerializeField]
    private Directions _attackDirection;
    [SerializeField]
    private float _attackPower = 1;
    [SerializeField]
    private float _minimumAttackDistance = 0;
    [SerializeField]
    private float _maximumAttackDistance = 3;
    [SerializeField]
    private bool _penetrateDefence = false;
    [SerializeField]
    private int _weight = 1;
    [SerializeField]
    private string[] _animationNames;
    [SerializeField]
    private float _animationTime = 1;
    [SerializeField]
    private HitDetection[] _attackColliders;
    [SerializeField]
    private float _turnDegrees = 45f; // Degrees to turn during attack

    private NPCBehaviourStateManager _manager;
    private bool _isActive;

    public Directions AttackDirection => _attackDirection;

    public float MinimumDistance => _minimumAttackDistance;

    public float MaximumDistance => _maximumAttackDistance;

    public NPCBehaviourStates State => NPCBehaviourStates.Attack;

    public int Weight => _weight;

    public string[] AnimationNames => _animationNames;

    public NPCBehaviourStateManager StateManager => _manager;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (value != _isActive)
                _isActive = value;
        }
    }

    public HitDetection[] AttackColliders => _attackColliders;

    public float AttackPower => _attackPower;

    public bool PenetrateDefence => _penetrateDefence;

    private void Awake()
    {
        _manager = GetComponent<NPCBehaviourStateManager>();
    }

    public void EndState(object data = null)
    {
        _isActive = false;
        StopAllCoroutines();
        _manager.AnimationHanler.SetAnimationBool(_animationNames[0], false);
    }

    public void StartState(object data = null)
    {
        StartCoroutine(TurnAndExecuteAttack());
        _manager.AnimationHanler.SetAnimationBool(_animationNames[0], true);
    }

    private IEnumerator TurnAndExecuteAttack()
    {
        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, _turnDegrees, 0) * initialRotation;
        float elapsedTime = 0f;

        // Start the attack and turn simultaneously
        foreach (HitDetection hitCollider in _attackColliders)
            hitCollider.SetAttacking(true, _penetrateDefence, _attackPower);

        while (elapsedTime < _animationTime)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / _animationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (HitDetection hitCollider in _attackColliders)
            hitCollider.SetAttacking(false, _penetrateDefence, _attackPower);

        transform.rotation = targetRotation;

        _manager.DoNewAttack.Invoke(this, EventArgs.Empty);
    }
}