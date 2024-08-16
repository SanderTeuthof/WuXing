using System.Collections;
using System;
using UnityEngine;
using UnityEngine.AI;

public class RunAndAttack : MonoBehaviour, INPCAttackState
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
    [SerializeField, Tooltip("This takes a walk animation at position 0 and the attack animation at position 1")]
    private string[] _animationNames;
    [SerializeField]
    private float _animationTime = 1;
    [SerializeField]
    private HitDetection[] _attackColliders;
    [SerializeField]
    private float _moveSpeed = 2;
    [SerializeField]
    private float _stopDistance = 1.5f;

    private float _originalMoveSpeed;

    private NPCBehaviourStateManager _manager;
    private NavMeshAgent _navMeshAgent;
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
        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
        {
            Debug.LogError("RunAndAttack requires a NavMeshAgent component.");
        }

        _originalMoveSpeed = _navMeshAgent.speed;
    }

    public void EndState(object data = null)
    {
        _navMeshAgent.speed = _originalMoveSpeed;
        _navMeshAgent.SetDestination(transform.position);
        _isActive = false;
        StopAllCoroutines();
        foreach(string animationName in AnimationNames) 
            _manager.AnimationHanler.SetAnimationBool(animationName, false);
    }

    public void StartState(object data = null)
    {
        _navMeshAgent.speed = _moveSpeed;
        StartCoroutine(RunToTargetAndAttack(data as Transform));
        _manager.AnimationHanler.SetAnimationBool(_animationNames[0], true);
    }

    private IEnumerator RunToTargetAndAttack(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("No target provided for RunAndAttack.");
            yield break;
        }

        // Start moving towards the target
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(target.position);

        yield return null; //give agent time to calculate distance

        // Wait until the NPC is close enough to the target
        while (_navMeshAgent.remainingDistance > _stopDistance)
        {
            yield return null; // Wait for the next frame
        }

        // Stop the NavMeshAgent
        _navMeshAgent.SetDestination(transform.position);

        _manager.AnimationHanler.SetAnimationBool(_animationNames[0], false);
        _manager.AnimationHanler.SetAnimationBool(_animationNames[1], true);

        // Start the attack animation and enable the attack colliders
        foreach (HitDetection hitCollider in _attackColliders)
            hitCollider.SetAttacking(true, _penetrateDefence, _attackPower);

        yield return new WaitForSeconds(_animationTime / 2);

        _manager.AnimationHanler.SetAnimationBool(_animationNames[1], false);

        yield return new WaitForSeconds(_animationTime / 2);

        // Disable the attack colliders after the attack is completed
        foreach (HitDetection hitCollider in _attackColliders)
            hitCollider.SetAttacking(false, _penetrateDefence, _attackPower);

        // Notify the manager that the attack is done
        _manager.DoNewAttack.Invoke(this, EventArgs.Empty);
    }
}