public interface ICard
{
    Element Element { get; }
    IUseSpell UseSpell { get; }
    IDiscardSpell DiscardSpell { get; }
}
