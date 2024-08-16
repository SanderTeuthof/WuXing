using UnityEngine;

public abstract class DiscardSpell : ScriptableObject, IDiscardSpell
{
    public abstract void Execute(SpellManager manager);
}