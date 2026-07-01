namespace Augments
{
    public class LifelineAugment : Augment
    {
        public override string Id => "lifeline";
        public override string DisplayName => "Lifeline";
        public override string Description =>
            $"When a nearby teammate would die, they survive on {AugmentText.HP("1 HP")} instead. " +
            $"{AugmentText.Cooldown("90s cooldown")} per player.";
        public override AugmentRarity Rarity => AugmentRarity.Legendary;
        public override AugmentClass Class => AugmentClass.Support;
        public override bool HasAuraEffect => true;
        // No hooks — pull check runs in AugmentPlayer.PreKill on the dying player's client.
    }
}
