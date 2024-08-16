using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(NPCBehaviourIdleStatesManager), typeof(NPCBehaviourStateManager))]
public class NPCWanderState : MonoBehaviour, INPCBehaviourState
{
    [SerializeField]
    private int _weight = 1;
    [SerializeField]
    private string[] _animationNames;
    [SerializeField]
    private float _speed = 1;

    [SerializeField]
    private Transform _centerLocation;
    [SerializeField]
    private float _minDistance = 0;
    [SerializeField]
    private float _maxDistance = 2;
    
    private NavMeshAgent _agent;
    private float _originalSpeed;

    public NPCBehaviourStates State => NPCBehaviourStates.Idle;

    private NPCBehaviourStateManager _stateManager;
    public NPCBehaviourStateManager StateManager { get => _stateManager; }
    private bool _isActive = false;
    public bool IsActive 
    { 
        get => _isActive; 
        set 
        {
            if (value != _isActive)
                _isActive = value;
        }
    }
    public int Weight => _weight;

    public string[] AnimationNames => _animationNames;

    private void Awake()
    {
        _stateManager = GetComponent<NPCBehaviourStateManager>();
        _agent = GetComponent<NavMeshAgent>();
        _originalSpeed = _agent.speed;
        if (_centerLocation == null)
            _centerLocation = transform;
    }

    public void StartState(object data = null)
    {
        _isActive = true;
        _agent.speed = _speed;
        StartCoroutine(StartIdleWander());
        _stateManager.AnimationHanler.SetAnimationBool(_animationNames[0], true);
    }

    public void EndState(object data = null)
    {
        _isActive = false;
        StopAllCoroutines();
        _agent.SetDestination(transform.position);
        _agent.speed = _originalSpeed;
        _stateManager.AnimationHanler.SetAnimationBool(_animationNames[0], false);
    }


    private IEnumerator StartIdleWander()
    {
        Vector3 centre = _centerLocation.position;

        Vector2 direction2D = UnityEngine.Random.insideUnitCircle;
        Vector3 direction = new Vector3(direction2D .x, 0 , direction2D.y) * UnityEngine.Random.Range(_minDistance, _maxDistance);
        Vector3 target = centre + direction;

        _agent.SetDestination(target);
        yield return null; // give the agent time to calculate

        while(_agent.remainingDistance > _agent.stoppingDistance)
            yield return null;

        _stateManager.AnimationHanler.SetAnimationBool(_animationNames[0], false);
        _stateManager.SetNewState(NPCBehaviourStates.Idle);
    }
}
