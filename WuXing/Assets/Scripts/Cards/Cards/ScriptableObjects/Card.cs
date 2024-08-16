using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card")]
public class Card : ScriptableObject, ICard
{
    [SerializeField]
    private Element element;

    [SerializeField]
    private ScriptableObject useSpell; // We will use Unity's serialization system, casting later

    [SerializeField]
    private ScriptableObject discardSpell; // Same as above

    public Element Element => element;
    public IUseSpell UseSpell => useSpell as IUseSpell;
    public IDiscardSpell DiscardSpell => discardSpell as IDiscardSpell;
}