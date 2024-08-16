using UnityEngine;

[CreateAssetMenu(fileName = "NewSummonObjectSpell", menuName = "Spell/Use/Summon Object")]
public class SummonObjectSpell : UseSpell
{
    [SerializeField]
    private GameObject _objectToSummon;
    [SerializeField]
    private int _numberOfObjects = 1;

    private Transform _summonPoint; // Set this up dynamically in the SpellController

    public override void Execute(SpellManager manager)
    {
        _summonPoint = manager.GetSpawnLocation();

        //Do logic for spawning multiple objects

        GameObject summonedObject = Instantiate(_objectToSummon, _summonPoint.position, _summonPoint.rotation);
        ISpell spellComponent = summonedObject.GetComponent<ISpell>();
        spellComponent.SetAttack(manager.NextSpellStrengthenEffects.DamageMultiplier);
        spellComponent.SetSpeed(manager.NextSpellStrengthenEffects.SpeedMultiplier);
        spellComponent.SetPenetrating(manager.NextSpellStrengthenEffects.Penetrating);
    }
}
