using Terraria;
using Terraria.ModLoader;

namespace Augments
{
	public class WhipMasterAugment : Augment
	{
		public override string Id => "whip_master";
		public override string DisplayName => "Whip Master";
		public override string Description =>
			"Whips gain 15% increased attack speed and 20% increased range.";

		public override AugmentRarity Rarity => AugmentRarity.Common;
		public override AugmentClass Class => AugmentClass.Summon;

		private const float AttackSpeedBonus = 0.15f;
		private const float RangeMultiplierBonus = 1.2f;

		public override void UpdateEquips(Player player)
		{
			player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += AttackSpeedBonus;
		}

		public override void OnProjectileSpawn(Player player, Projectile projectile)
		{
			if (projectile.DamageType == DamageClass.SummonMeleeSpeed)
				projectile.WhipSettings.RangeMultiplier *= RangeMultiplierBonus;
		}
	}
}
