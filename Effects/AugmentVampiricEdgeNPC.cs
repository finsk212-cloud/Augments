using Terraria.ModLoader;

namespace Augments
{
	// Marks an NPC as "last hit by this player's melee damage." Needed so
	// Vampiric Edge also credits kills finished off by a melee-inflicted DoT
	// (e.g. a bleed/poison applied by an earlier hit ticking the target down)
	// - those kills never fire OnHitNPCWithItem/Proj for the killing blow, so
	// a plain "target.life <= 0 inside OnHit" check misses them.
	public class AugmentVampiricEdgeNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private int taggedPlayerIndex = -1;

		public void TagMeleeHit(int playerIndex)
		{
			taggedPlayerIndex = playerIndex;
		}

		public bool IsTaggedBy(int playerIndex)
		{
			return taggedPlayerIndex == playerIndex;
		}

		public void ClearTag()
		{
			taggedPlayerIndex = -1;
		}
	}
}
