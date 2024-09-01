using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthRepresentation : MonoBehaviour
{
    [SerializeField]
    private FloatReference _playerCurrentHealth;
    [SerializeField]
    private FloatReference _playerMaxHealth;

    private void Start()
    {
        _playerCurrentHealth.UseEvent();
        _playerCurrentHealth.ValueChanged += UpdateHealth;
    }

    private void UpdateHealth(object sender, EventArgs e)
    {
        Vector3 newScale = Vector3.one;
        newScale.x = _playerCurrentHealth.value / _playerMaxHealth.value;
        transform.localScale = newScale;
    }

    private void OnDestroy()
    {
        _playerCurrentHealth.OnDestroy();
    }
}
