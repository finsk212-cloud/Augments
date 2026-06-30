using Terraria;
using Terraria.ModLoader;

namespace Augments
{
	// Tracks a custom movement-slow effect per NPC. Vanilla's BuffID.Slow is
	// hardcoded to only affect players - AddBuff will happily attach it to an
	// NPC, but it's a no-op - so Time Warp needs its own tracked slow instead.
	public class AugmentSlowNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private int ticksRemaining;
		private float slowPercent;

		// Call this to start (or refresh) a slow on this NPC.
		public void ApplySlow(int durationTicks, float percent)
		{
			ticksRemaining = durationTicks;
			slowPercent = percent;
		}

		public override void PostAI(NPC npc)
		{
			if (ticksRemaining <= 0)
				return;

			ticksRemaining--;
			npc.velocity *= 1f - slowPercent;
		}
	}
}
