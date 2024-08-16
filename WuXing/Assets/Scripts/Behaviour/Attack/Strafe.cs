using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NPCBehaviourAttackStatesManager), typeof(NPCBehaviourStateManager), typeof(NavMeshAgent))]
public class Strafe : MonoBehaviour, INPCAttackState
{
    [SerializeField]
    private bool _strafeLeft = true; // Determines the strafe direction (true = left, false = right)
    [SerializeField]
    private float _attackPower = 1;
    [SerializeField]
    private float _minimumAttackDistance = 0;
    [SerializeField]
    private float _maximumAttackDistance = 3;
    [SerializeField]
    private float _strafeSpeed = 3;
    [SerializeField]
    private float _minimumStrafeTime = 1;
    [SerializeField]
    private float _maximumStrafeTime = 3;
    [SerializeField]
    private EasingType _moveType;
    [SerializeField]
    private float _standStillAtEndTime = 0.3f;
    [SerializeField]
    private int _weight = 1;
    [SerializeField]
    private string[] _animationNames;
    [SerializeField]
    private bool _penetrateDefence = false;
    [SerializeField]
    private HitDetection[] _attackColliders;

    private NPCBehaviourStateManager _manager;
    private bool _isActive;
    private NavMeshAgent _agent;
    private float _originalRotSpeed;

    public Directions AttackDirection => _strafeLeft ? Directions.Left : Directions.Right;

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
        _agent = GetComponent<NavMeshAgent>();
        _originalRotSpeed = _agent.angularSpeed;
    }

    public void EndState(object data = null)
    {
        _isActive = false;
        _agent.angularSpeed = _originalRotSpeed;
        StopAllCoroutines();
        foreach (HitDetection hitCollider in _attackColliders)
            hitCollider.SetAttacking(false, _penetrateDefence, _attackPower);
        _manager.AnimationHanler.SetAnimationBool(_animationNames[0], false);
    }

    public void StartState(object data = null)
    {
        _isActive = true;
        _agent.angularSpeed = 0;
        _agent.SetDestination(transform.position);
        StartCoroutine(DoStrafe(data as Transform));
        _manager.AnimationHanler.SetAnimationBool(_animationNames[0], true);
    }

    private IEnumerator DoStrafe(Transform target)
    {
        if (target == null)
            _manager.DoNewAttack.Invoke(this, EventArgs.Empty);

        float strafeTime = UnityEngine.Random.Range(_minimumStrafeTime, _maximumStrafeTime);

        Vector3 movement;

        float time = 0;
        while (time < strafeTime)
        {
            time += Time.deltaTime;
            if (_strafeLeft)
                movement = Vector3.Lerp(Vector3.zero, -transform.right, _strafeSpeed * Time.deltaTime);
            else
                movement = Vector3.Lerp(Vector3.zero, transform.right, _strafeSpeed * Time.deltaTime);

            _agent.Move(movement);

            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 8); //magic rotation speed number

            yield return null;
        }

        yield return new WaitForSeconds(_standStillAtEndTime);

        _manager.DoNewAttack.Invoke(this, EventArgs.Empty);
    }

    public void StartHitColliders()
    {
        foreach (HitDetection hitCollider in _attackColliders)
            hitCollider.SetAttacking(true, _penetrateDefence, _attackPower);
    }

    public void StopHitColliders()
    {
        foreach (HitDetection hitCollider in _attackColliders)
            hitCollider.SetAttacking(false, _penetrateDefence, _attackPower);
    }
}