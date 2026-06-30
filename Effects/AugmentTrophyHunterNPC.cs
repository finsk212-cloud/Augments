using Terraria.ModLoader;

namespace Augments
{
	// Marks an NPC as "last hit by this player's melee damage," same shape as
	// AugmentVampiricEdgeNPC but kept separate so the two augments' tag/clear
	// timing can never interfere with each other when both are owned at once.
	// Needed so a melee-tagged kill still counts even when finished off by a
	// melee-inflicted DoT (e.g. Bloodletter's bleed) - those kills never fire
	// OnHitNPCWithItem/Proj for the killing blow itself.
	public class AugmentTrophyHunterNPC : GlobalNPC
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
