using UnityEngine;

public interface ISpell
{
    Element Element { get; }
    void SetCaster(Transform caster);
    void SetSpeed(float speedMultiplier);
    void SetAttack(float attackMultiplier);
    void SetPenetrating(bool penetrating);
    void SetTarget(Transform target);
    void SetTracking(bool shouldTrack);
    void Execute();
}

