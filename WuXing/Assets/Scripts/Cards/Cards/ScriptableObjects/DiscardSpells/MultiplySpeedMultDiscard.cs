using UnityEngine;

[CreateAssetMenu(fileName = "MultiplySpeedMultDiscard", menuName = "Spell/Discard/Multiply Speed Multiplier")]
public class MultiplySpeedMultDiscard : UseSpell, IDiscardSpell
{
    [SerializeField]
    private float _speedMultiplier = 1;

    public override void Execute(SpellManager manager)
    {
        manager.NextSpellStrengthenEffects.SpeedMultiplier *= _speedMultiplier;
    }
}