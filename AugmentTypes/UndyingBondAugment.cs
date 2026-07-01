namespace Augments
{
    public class UndyingBondAugment : Augment
    {
        public override string Id => "undying_bond";
        public override string DisplayName => "Undying Bond";
        public override string Description =>
            $"While you are alive and have the {AugmentText.SupportClass("Support Class")} buff, dead teammates respawn at your location instead of their spawn point.";
        public override AugmentRarity Rarity => AugmentRarity.Legendary;
        public override AugmentClass Class => AugmentClass.Support;
        // HasAuraEffect stays false — no fixed radius, works across the whole map.
        // No hooks — respawn redirect runs in AugmentPlayer.UpdateDead on the dead player's client.
    }
}
