using Terraria;
using Terraria.ModLoader;

namespace Augments
{
	// Tracks a custom bleed-over-time effect per NPC, using the same pattern
	// vanilla debuffs (Poison, On Fire, etc.) actually use: a continuously
	// negative lifeRegen for the whole duration, not a single momentary spike.
	//
	// Autoload disabled and registered manually in Augments.Load(), before
	// AugmentFesteringWoundsNPC - that amplifier's UpdateLifeRegen must read
	// this bleed's contribution to lifeRegen AFTER it's been applied for the
	// tick, and tModLoader doesn't guarantee autoload order between two
	// separate GlobalNPCs.
	[Autoload(false)]
	public class AugmentBleedNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private int ticksRemaining;
		private int damagePerSecond;

		// Call this to start (or refresh) a bleed on this NPC.
		public void ApplyBleed(int durationTicks, int dps)
		{
			ticksRemaining = durationTicks;
			damagePerSecond = dps;
		}

		// This custom bleed never sets a vanilla BuffID, so target.HasBuff()
		// can't see it - anything that needs to detect "is this NPC currently
		// bleeding" (e.g. Volatile Rounds' DoT synergy check) has to ask here
		// directly instead.
		public bool IsActive => ticksRemaining > 0;

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (ticksRemaining <= 0)
				return;

			ticksRemaining--;

			// lifeRegen applies at HALF its value per second, so we subtract
			// twice the damage-per-second we actually want dealt.
			npc.lifeRegen -= damagePerSecond * 2;

			// This is what makes the popup number appear over the NPC.
			if (damagePerSecond > damage)
				damage = damagePerSecond;
		}
	}
}
