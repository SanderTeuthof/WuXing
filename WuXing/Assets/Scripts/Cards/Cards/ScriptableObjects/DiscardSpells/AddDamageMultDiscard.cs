using UnityEngine;

[CreateAssetMenu(fileName = "AddDamageMultDiscard", menuName = "Spell/Discard/Add Damage Multiplier")]
public class AddDamageMultDiscard : UseSpell, IDiscardSpell
{
    [SerializeField]
    private float _damageMultiplier = 1;

    public override void Execute(SpellManager manager)
    {
        manager.NextSpellStrengthenEffects.DamageMultiplier += _damageMultiplier;
    }
}