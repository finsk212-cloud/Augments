using Terraria;
using Terraria.ModLoader;

namespace Augments
{
	public class WandDisciplineAugment : Augment
	{
		public override string Id => "wand_discipline";
		public override string DisplayName => "Wand Discipline";
		public override string Description =>
			$"Magic weapons cost {AugmentText.Mana("10% less mana")} to use.";

		public override AugmentRarity Rarity => AugmentRarity.Common;
		public override AugmentClass Class => AugmentClass.Magic;

		private const float ManaCostMultiplier = 0.9f;

		public override void ModifyManaCost(Player player, Item item, ref float reduce, ref float mult)
		{
			if (item.DamageType == DamageClass.Magic)
				mult *= ManaCostMultiplier;
		}
	}
}
