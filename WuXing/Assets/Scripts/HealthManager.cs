using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField]
    private float _maxHealth = 10;

    private float _health;

    private IDestroyable _destroyable;
    private ObjectInfo _objectInfo;

    private void Awake()
    {
        _health = _maxHealth;
        _destroyable = GetComponent<IDestroyable>();
        _objectInfo = GetComponent<ObjectInfo>();
    }

    public void TakeDamage(float damage, Element incomingElement)
    {
        float actualDamage = damage * ElementalInteractions.GetDamageMultiplier(incomingElement, _objectInfo.ElementAllignment);
        _health = Mathf.Max(0, _health - actualDamage);

        ChechDeath();
    }

    public void Heal(float healAmount)
    {
        _health = Mathf.Min(_maxHealth, _health + healAmount);
    }

    private void ChechDeath()
    {
        if (_health == 0)
        {
            _destroyable.Destroy();
        }
    }
}

public interface IDestroyable
{
    void Destroy();   
}