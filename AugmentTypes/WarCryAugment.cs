namespace Augments
{
	public class WarCryAugment : Augment
	{
		public override string Id => "warcry";
		public override string DisplayName => "Warcry";
		public override string Description =>
			$"Nearby teammates gain {AugmentText.BonusDamage("+10% damage")} while you are within range.";

		public override AugmentRarity Rarity => AugmentRarity.Common;
		public override AugmentClass Class => AugmentClass.Support;
		public override bool HasAuraEffect => true;

		// No hooks - the aura is pull-based. Each nearby player checks for this
		// augment in AugmentPlayer.PostUpdate and applies the buff to themselves.
	}
}
