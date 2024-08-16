using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCBehaviourAttackStatesManager), typeof(NPCBehaviourStateManager))]
public class BasicAttack : MonoBehaviour, INPCAttackState
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
            if (value !=  _isActive) 
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
        foreach (HitDetection hitCollider in _attackColliders)
            hitCollider.SetAttacking(false, _penetrateDefence, _attackPower);
        _manager.AnimationHanler.SetAnimationBool(_animationNames[0], false);
    }

    public void StartState(object data = null)
    {
        _isActive = true;
        StartCoroutine(DoAttack());
        _manager.AnimationHanler.SetAnimationBool(_animationNames[0], true);
    }

    private IEnumerator DoAttack()
    {
        //set the attack collider at the beginning, this can be improved by letting it do this with animation events
        foreach (HitDetection hitCollider in _attackColliders)
            hitCollider.SetAttacking(true, _penetrateDefence, _attackPower);

        yield return new WaitForSeconds(_animationTime / 2);

        _manager.AnimationHanler.SetAnimationBool(_animationNames[0], false);

        yield return new WaitForSeconds(_animationTime / 2);

        foreach (HitDetection hitCollider in _attackColliders)
            hitCollider.SetAttacking(false, _penetrateDefence, _attackPower);

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
