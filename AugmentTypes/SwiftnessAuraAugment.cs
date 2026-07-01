namespace Augments
{
    public class SwiftnessAuraAugment : Augment
    {
        public override string Id => "swiftness_aura";
        public override string DisplayName => "Swiftness Aura";
        public override string Description =>
            $"Nearby teammates gain {AugmentText.MovementSpeed("+15% movement speed")}.";
        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Support;
        public override bool HasAuraEffect => true;
        // No hooks — pull-based architecture (AugmentPlayer.PostUpdateRunSpeeds)
    }
}
