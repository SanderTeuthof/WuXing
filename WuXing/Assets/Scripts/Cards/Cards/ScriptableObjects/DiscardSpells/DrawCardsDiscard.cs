using UnityEngine;

[CreateAssetMenu(fileName = "DrawCardsDiscard", menuName = "Spell/Discard/Draw Cards")]
public class DrawCardsDiscard : UseSpell, IDiscardSpell
{
    [SerializeField]
    private int _cardsToDraw = 2;

    public override void Execute(SpellManager manager)
    {
        manager.DrawNewCards(_cardsToDraw);
    }
}
