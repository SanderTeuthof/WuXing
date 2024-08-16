using System.Collections.Generic;

public static class ElementalInteractions
{
    private static readonly List<Interaction> _aggressionInteractions = new List<Interaction>
    {
        new Interaction(Element.Fire, Element.Metal, 2.0f),
        new Interaction(Element.Water, Element.Fire, 2.0f),
        new Interaction(Element.Earth, Element.Water, 2.0f),
        new Interaction(Element.Wood, Element.Earth, 2.0f),
        new Interaction(Element.Metal, Element.Wood, 2.0f),

        new Interaction(Element.Metal, Element.Fire, 1.0f),
        new Interaction(Element.Fire, Element.Water, 1.0f),
        new Interaction(Element.Water, Element.Earth, 1.0f),
        new Interaction(Element.Earth, Element.Wood, 1.0f),
        new Interaction(Element.Wood, Element.Metal, 1.0f)
    };

    private static readonly List<Interaction> _damageInteractions = new List<Interaction>
    {
        new Interaction(Element.Fire, Element.Metal, 2.0f),
        new Interaction(Element.Water, Element.Fire, 2.0f),
        new Interaction(Element.Earth, Element.Water, 2.0f),
        new Interaction(Element.Wood, Element.Earth, 2.0f),
        new Interaction(Element.Metal, Element.Wood, 2.0f),

        new Interaction(Element.Metal, Element.Fire, 0.5f),
        new Interaction(Element.Fire, Element.Water, 0.5f),
        new Interaction(Element.Wood, Element.Metal, 0.5f),
        new Interaction(Element.Earth, Element.Wood, 0.5f),
        new Interaction(Element.Water, Element.Earth, 0.5f),

        new Interaction(Element.Water, Element.Wood, 0.25f),
        new Interaction(Element.Wood, Element.Fire, 0.25f),
        new Interaction(Element.Fire, Element.Earth, 0.25f),
        new Interaction(Element.Earth, Element.Metal, 0.25f),
        new Interaction(Element.Metal, Element.Water, 0.25f)
    };

    public static float GetAggressionValue(Element attacker, Element target)
    {
        foreach (var interaction in _aggressionInteractions)
        {
            if (interaction.Attacker == attacker && interaction.Target == target)
                return interaction.Value;
        }
        return 0f; // Neutral interaction
    }

    public static float GetDamageMultiplier(Element attacker, Element target)
    {
        foreach (var interaction in _damageInteractions)
        {
            if (interaction.Attacker == attacker && interaction.Target == target)
                return interaction.Value;
        }
        return 1f; // Neutral interaction
    }

    private class Interaction
    {
        public Element Attacker { get; }
        public Element Target { get; }
        public float Value { get; }

        public Interaction(Element attacker, Element target, float value)
        {
            Attacker = attacker;
            Target = target;
            Value = value;
        }
    }
}
