﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCBehaviourAttackStatesManager : MonoBehaviour, INPCBehaviourStatesTypeManager
{
    [SerializeField]
    private INPCBehaviourState _backupState;

    private NPCBehaviourStates _behaviourState = NPCBehaviourStates.Attack;
    private List<INPCBehaviourState> _states = new();
    private List<INPCAttackState> _possibleAttacks;

    private int _totalWeight = 0;

    public NPCBehaviourStates StateType => _behaviourState;

    private void Awake()
    {
        GetCorrectStates();
    }

    private void GetCorrectStates()
    {
        List<INPCBehaviourState> allstates = GetComponents<INPCBehaviourState>().ToList();
        foreach (INPCBehaviourState state in allstates)
        {
            if (state.State == _behaviourState)
                _states.Add(state);
            if (_backupState == null && state.State == NPCBehaviourStates.Idle)
                _backupState = state;
        }
    }

    public INPCBehaviourState GetState(object data = null)
    {
        Transform target = data as Transform;

        if (target == null)
        {
            Debug.LogWarning("No target given to attack!");
            return _backupState;
        }

        Vector3 directionToTarget = transform.position - target.position;

        float distance = Vector3.Magnitude(directionToTarget);

        Directions direction = GetClosestDirection(directionToTarget.normalized);

        _possibleAttacks = new();

        foreach (INPCBehaviourState state in _states)
        {
            if (state is INPCAttackState attack && ((attack.AttackDirection.HasFlag(direction) && attack.MinimumDistance < distance && distance < attack.MaximumDistance)))
            {
                _possibleAttacks.Add(attack);
            }
        }

        if (_possibleAttacks.Count == 0)
        {
            Debug.LogWarning("No possible attack found");
            return _backupState;
        }
        if (_possibleAttacks.Count == 1)
        {
            return _possibleAttacks[0];
        }

        UpdateTotalWeight();

        int randomWeight = UnityEngine.Random.Range(0, _totalWeight);
        int currentWeight = 0;

        foreach (var state in _possibleAttacks)
        {
            currentWeight += state.Weight;
            if (randomWeight < currentWeight)
            {
                return state;
            }
        }
        return _possibleAttacks[0];
    }


    public void UpdateTotalWeight()
    {
        _totalWeight = _possibleAttacks.Sum(state => state.Weight);
    }

    private Directions GetClosestDirection(Vector3 direction)
    {
        Directions closestDirection = Directions.None;

        Vector3 checkVector = direction;

        float closeness = -1f;

        for (int i = 0; i < 6; i++)
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
                return -transform.forward;
            case 1:
                return transform.forward;
            case 2:
                return transform.right;
            case 3:
                return -transform.right;
            case 4:
                return -transform.up;
            case 5:
                return transform.up;
            default:
                return Vector3.zero;
        }
    }
}
