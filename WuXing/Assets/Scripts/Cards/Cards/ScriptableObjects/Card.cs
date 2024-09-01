using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card")]
public class Card : ScriptableObject, ICard
{
    [SerializeField]
    private Element _element;
    [SerializeField]
    private GameObject _cardRepresentation;
    [SerializeField]
    private ScriptableObject _useSpell; // We will use Unity's serialization system, casting later

    [SerializeField]
    private ScriptableObject _discardSpell; // Same as above

    public Element Element => _element;
    public IUseSpell UseSpell => _useSpell as IUseSpell;
    public IDiscardSpell DiscardSpell => _discardSpell as IDiscardSpell;

    public GameObject CardRepresentation => _cardRepresentation;
}