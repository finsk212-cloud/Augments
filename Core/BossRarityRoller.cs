using Terraria;

namespace Augments
{
	// Rolls a weighted AugmentRarity for a given boss bracket. Weights are
	// per-mille... well, per-cent - each bracket's weights sum to 100.
	public static class BossRarityRoller
	{
		public static AugmentRarity Roll(RarityBracket bracket)
		{
			int roll = Main.rand.Next(100);

			switch (bracket)
			{
				case RarityBracket.WallOfFlesh:
					if (roll < 60) return AugmentRarity.Common;
					if (roll < 95) return AugmentRarity.Rare;
					return AugmentRarity.Epic;

				case RarityBracket.HardmodePrePlantera:
					if (roll < 40) return AugmentRarity.Common;
					if (roll < 85) return AugmentRarity.Rare;
					return AugmentRarity.Epic;

				case RarityBracket.PlanteraPlus:
					if (roll < 20) return AugmentRarity.Common;
					if (roll < 65) return AugmentRarity.Rare;
					if (roll < 95) return AugmentRarity.Epic;
					return AugmentRarity.Legendary;

				case RarityBracket.Endgame:
					if (roll < 25) return AugmentRarity.Rare;
					if (roll < 75) return AugmentRarity.Epic;
					return AugmentRarity.Legendary;

				case RarityBracket.PreHardmode:
				default:
					if (roll < 85) return AugmentRarity.Common;
					return AugmentRarity.Rare;
			}
		}
	}
}
