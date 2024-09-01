using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField]
    protected float _maxHealth = 10;

    protected float _health;

    private IDestroyable _destroyable;
    private ObjectInfo _objectInfo;

    private void Awake()
    {
        _health = _maxHealth;
        _destroyable = GetComponent<IDestroyable>();
        _objectInfo = GetComponent<ObjectInfo>();
    }

    public virtual void TakeDamage(float damage, Element incomingElement)
    {
        Debug.Log("Damage taken!");
        float actualDamage = damage * ElementalInteractions.GetDamageMultiplier(incomingElement, _objectInfo.ElementAllignment);
        _health = Mathf.Max(0, _health - actualDamage);

        CheckDeath();
    }

    public virtual void Heal(float healAmount)
    {
        _health = Mathf.Min(_maxHealth, _health + healAmount);
    }

    private void CheckDeath()
    {
        if (_health == 0)
        {
            _destroyable.Destroy();
        }
    }
}
