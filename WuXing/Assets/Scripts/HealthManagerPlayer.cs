using UnityEngine;

public class HealthManagerPlayer : HealthManager
{
    [SerializeField]
    private FloatReference _playerCurrentHealth;
    [SerializeField]
    private FloatReference _playerMaxHealth;

    private void Start()
    {
        _playerCurrentHealth.SetValue(_maxHealth);
        _playerMaxHealth.SetValue(_maxHealth);
    }

    public override void TakeDamage(float damage, Element incomingElement)
    {
        base.TakeDamage(damage, incomingElement);
        _playerCurrentHealth.SetValue(_health);
    }

    public override void Heal(float healAmount)
    {
        base.Heal(healAmount);
        _playerCurrentHealth.SetValue(_health);
    }
}