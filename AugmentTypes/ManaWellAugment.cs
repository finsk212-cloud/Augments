namespace Augments
{
    public class ManaWellAugment : Augment
    {
        public override string Id => "mana_well";
        public override string DisplayName => "Mana Well";
        public override string Description =>
            $"Nearby teammates regenerate mana {AugmentText.Mana("30% faster")}.";
        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Support;
        public override bool HasAuraEffect => true;
        // No hooks — pull-based architecture (AugmentPlayer.UpdateEquips)
    }
}
