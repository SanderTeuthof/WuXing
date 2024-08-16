using UnityEngine;

[CreateAssetMenu(fileName = "MultiplyDamageMultDiscard", menuName = "Spell/Discard/Multiply Damage Multiplier")]
public class MultiplyDamageMultDiscard : UseSpell, IDiscardSpell
{
    [SerializeField]
    private float _damageMultiplier = 1;

    public override void Execute(SpellManager manager)
    {
        manager.NextSpellStrengthenEffects.DamageMultiplier *= _damageMultiplier;
    }
}
