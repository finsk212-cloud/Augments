using Terraria;
using Terraria.ModLoader;

namespace Augments
{
	public class BossAugmentDrop : GlobalNPC
	{
		// Runs server-side / singleplayer whenever any NPC dies.
		public override void OnKill(NPC npc)
		{
			if (!npc.boss)
				return;

			RarityBracket bracket = BossTierMap.GetBracket(npc.type);

			foreach (Player player in Main.player)
			{
				if (!player.active)
					continue;

				AugmentRewardLogic.GrantReward(player, bracket);
			}
		}
	}
}
