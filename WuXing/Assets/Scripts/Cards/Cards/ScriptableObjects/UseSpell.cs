using UnityEngine;

public abstract class UseSpell : ScriptableObject, IUseSpell
{
    public abstract void Execute(SpellManager manager);
}