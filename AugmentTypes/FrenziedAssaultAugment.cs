using Terraria;
using Terraria.ModLoader;

namespace Augments
{
	public class FrenziedAssaultAugment : Augment
	{
		public override string Id => "frenzied_assault";
		public override string DisplayName => "Frenzied Assault";
		public override string Description =>
			$"Melee crits grant a stacking attack speed buff: {AugmentText.MovementSpeed("+7% per stack")}, up to " +
			$"{AugmentText.MovementSpeed("+35% at 5 stacks")}. Stacks reset after {AugmentText.Duration("3 seconds")}";

		public override AugmentRarity Rarity => AugmentRarity.Epic;
		public override AugmentClass Class => AugmentClass.Melee;

		private const int ResetWindowTicks = 180;
		private const int MaxStacks = 5;
		private const float AttackSpeedPerStack = 0.07f;

		private int stacks;
		private int resetTimer;

		public override void OnUpdate(Player player)
		{
			if (resetTimer <= 0)
				return;

			resetTimer--;
			if (resetTimer == 0)
				stacks = 0;
		}

		public override void UpdateEquips(Player player)
		{
			if (stacks > 0)
				player.GetAttackSpeed(DamageClass.Melee) += stacks * AttackSpeedPerStack;
		}

		public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
		{
			if (item.CountsAsClass(DamageClass.Melee) && hit.Crit)
				RegisterCrit();
		}

		public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
		{
			if (proj.CountsAsClass(DamageClass.Melee) && hit.Crit)
				RegisterCrit();
		}

		private void RegisterCrit()
		{
			if (stacks < MaxStacks)
				stacks++;

			resetTimer = ResetWindowTicks;
		}
	}
}
