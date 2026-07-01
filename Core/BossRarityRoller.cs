using Terraria;

namespace Augments
{
	public readonly struct RarityRollChances
	{
		public readonly int Common;
		public readonly int Rare;
		public readonly int Epic;
		public readonly int Legendary;

		public RarityRollChances(int common, int rare, int epic, int legendary)
		{
			Common = common;
			Rare = rare;
			Epic = epic;
			Legendary = legendary;
		}
	}

	// Rolls a weighted AugmentRarity for a given boss bracket. Weights are
	// per-mille... well, per-cent - each bracket's weights sum to 100.
	public static class BossRarityRoller
	{
		public static AugmentRarity Roll(RarityBracket bracket)
		{
			RarityRollChances chances = GetChancesForBracket(bracket);
			int roll = Main.rand.Next(100);
			if (roll < chances.Common)
				return AugmentRarity.Common;
			roll -= chances.Common;
			if (roll < chances.Rare)
				return AugmentRarity.Rare;
			roll -= chances.Rare;
			if (roll < chances.Epic)
				return AugmentRarity.Epic;
			return AugmentRarity.Legendary;
		}

		private static RarityRollChances GetChancesForBracket(RarityBracket bracket)
		{
			return bracket switch
			{
				RarityBracket.PreHardmode => new RarityRollChances(85, 15, 0, 0),
				RarityBracket.EarlyHardmode => new RarityRollChances(55, 40, 5, 0),
				RarityBracket.PostMechs => new RarityRollChances(35, 40, 20, 5),
				RarityBracket.PostPlantera => new RarityRollChances(25, 35, 25, 15),
				RarityBracket.EarlyPostMoonLord => new RarityRollChances(20, 35, 30, 15),
				RarityBracket.LatePostMoonLord => new RarityRollChances(15, 25, 35, 25),
				RarityBracket.FinalCalamity => new RarityRollChances(10, 20, 30, 40),
				_ => new RarityRollChances(85, 15, 0, 0)
			};
		}
	}
}
