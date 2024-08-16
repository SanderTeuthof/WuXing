using System.Collections;
using UnityEngine;

public class FlyForward : MonoBehaviour, ISpell
{
    [SerializeField] 
    private Element _element;
    [SerializeField]
    private float _speed = 10f;
    [SerializeField]
    private float _attack = 10f;
    [SerializeField]
    private float _maxDuration = 5f;
    [SerializeField]
    private LayerMask _destroyLayers;
    [SerializeField]
    private LayerMask _attackLayers;
    [SerializeField]
    private bool _penetrateEnemies;

    private float _currentTime = 0f;
    private IDestroyable _destroyable;

    Element ISpell.Element => _element;

    private void Start()
    {
        Execute();
    }

    public void Execute()
    {
        _destroyable = GetComponent<IDestroyable>();
        StartCoroutine(Fly());
    }

    public void SetSpeed(float speed)
    {
        _speed *= speed;
    }
    public void SetAttack(float attack)
    {
        _attack *= attack;
    }

    public void SetPenetrating(bool penetrating)
    {
        _penetrateEnemies = _penetrateEnemies || penetrating;
    }

    public void SetTarget(Transform target) //not used
    {
        //not used
    } 

    public void SetTracking(bool shouldTrack) //not used
    {
        //not used
    }

    private IEnumerator Fly()
    {
        while (_currentTime < _maxDuration)
        {
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
            _currentTime += Time.deltaTime;

            yield return null;
        }

        _destroyable.Destroy(); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _destroyLayers) != 0)
        {
            _destroyable.Destroy();
            return;
        }

        // Check if the object hit is in the attack layers
        if (((1 << other.gameObject.layer) & _attackLayers) != 0)
        {
            // Attempt to get the HealthManager component from the root of the hit object
            HealthManager health = other.transform.root.GetComponent<HealthManager>();

            // Attempt to get the HitDetection component from the hit object
            HitDetection thingHit = other.GetComponent<HitDetection>();

            float damageMulti = 1.0f;

            // If thingHit is not null, get the damage multiplier from it
            if (thingHit != null)
            {
                damageMulti = thingHit.DamageMultiplier;
            }

            // If health is not null, apply damage to the hit object
            if (health != null)
            {
                health.TakeDamage(_attack * damageMulti, _element);
            }

            // If the spell is not penetrating or the hit object doesn't have health, destroy the spell
            if (!_penetrateEnemies || health == null)
            {
                _destroyable.Destroy();
            }
        }
    }
}