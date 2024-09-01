using System.Collections;
using UnityEngine;

public class RiseFromGround : MonoBehaviour, ISpell
{
    [SerializeField]
    private Element _element;
    [SerializeField]
    private float _moveUpDistance = 1;
    [SerializeField]
    private float _moveUpTime = 1;
    [SerializeField]
    private float _timeBeforeDisappear = 5;
    [SerializeField]
    private EasingType _moveType;
    [SerializeField]
    private float _attack = 10f;  // Assuming an attack property is necessary
    [SerializeField]
    private LayerMask _attackLayers;

    private IDestroyable _destroyable;
    private Transform _caster;
    private bool _penetrateEnemies;

    public Element Element => _element;

    private void Awake()
    {
        _destroyable = GetComponent<IDestroyable>();

        // Cast a ray downward to find the ground
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            // Set the position to the hit point
            transform.position = hit.point;
        }
        else
        {
            Debug.LogWarning("RiseFromGround: No ground detected below the spell. Position remains unchanged.");
        }

        Vector3 heightScale = transform.localScale;
        heightScale.y *= _moveUpDistance;
        transform.localScale = heightScale;
    }

    public void Execute()
    {
        StartCoroutine(MoveUp());
    }

    public void SetAttack(float attackMultiplier)
    {
        _attack *= attackMultiplier;
    }

    public void SetCaster(Transform caster)
    {
        _caster = caster;
    }

    public void SetPenetrating(bool penetrating)
    {
        _penetrateEnemies = _penetrateEnemies || penetrating;
    }

    public void SetSpeed(float speedMultiplier)
    {
        _moveUpTime /= speedMultiplier;
    }

    public void SetTarget(Transform target)
    {
        // Not used in this implementation
    }

    public void SetTracking(bool shouldTrack)
    {
        // Not used in this implementation
    }

    private IEnumerator MoveUp()
    {
        Vector3 originPos = transform.position;
        Vector3 targetPos = originPos;
        targetPos.y += _moveUpDistance;
        float time = 0;

        // Moving up
        while (time / _moveUpTime < 1)
        {
            time += Time.deltaTime;
            float t = EasingFunctions.Ease(_moveType, time / _moveUpTime);
            transform.position = Vector3.Lerp(originPos, targetPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(_timeBeforeDisappear);

        // Moving down (or another action before disappearing)
        time = 0;
        while (time / _moveUpTime < 1)
        {
            time += Time.deltaTime;
            float t = EasingFunctions.Ease(_moveType, time / _moveUpTime);
            transform.position = Vector3.Lerp(targetPos, originPos, t);
            yield return null;
        }

        _destroyable.Destroy();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == _caster) return;

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
