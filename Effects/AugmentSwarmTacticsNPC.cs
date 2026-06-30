using Terraria.ModLoader;

namespace Augments
{
	// Marks an NPC as "last hit by this player's summon damage." Swarm Tactics
	// needs this because a kill finished off by a minion-applied DoT (e.g.
	// Hornet Staff poison ticking the target down after the stinger hit
	// applied it) never fires OnHitNPCWithItem/Proj for the killing blow - the
	// debuff tick isn't a hit event - so a plain "target.life <= 0 inside
	// OnHit" check (like Vampiric Edge uses) misses it. Tagging on every
	// summon hit and checking the tag on kill credit (OnKillNPC) instead
	// covers both direct-hit kills and DoT-finished kills the same way.
	public class AugmentSwarmTacticsNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private int taggedPlayerIndex = -1;

		public void TagSummonHit(int playerIndex)
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
