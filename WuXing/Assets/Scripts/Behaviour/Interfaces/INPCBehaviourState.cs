﻿public interface INPCBehaviourState
{
    NPCBehaviourStates State { get; }
    int Weight {  get; }
    string[] AnimationNames { get; }
    NPCBehaviourStateManager StateManager { get; }
    bool IsActive { get; set; }
    void EndState(object data = null);
    void StartState(object data = null);

}

public interface INPCAttackState : INPCBehaviourState
{
    float AttackPower { get; }
    Directions AttackDirection { get; }
    float MinimumDistance { get; }
    float MaximumDistance { get; }
    bool PenetrateDefence { get; }
    HitDetection[] AttackColliders { get; }
}
