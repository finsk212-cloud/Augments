using Terraria;
using Terraria.ModLoader;

namespace Augments
{
	// Marks a coin item dropped by Lucky Find so its value is only credited
	// to the running total once the player actually picks it up - not the
	// instant it's spawned, since a dropped coin can despawn, fall in lava,
	// or get grabbed by something else first.
	public class AugmentBonusCoinItem : GlobalItem
	{
		public override bool InstancePerEntity => true;

		public bool IsLuckyFindBonus;

		public override bool OnPickup(Item item, Player player)
		{
			if (IsLuckyFindBonus)
			{
				var ap = player.GetModPlayer<AugmentPlayer>();
				if (ap.HasAugment("lucky_find"))
					ap.LuckyFindCopperGained += item.stack * LuckyFindAugment.CopperPerSilver;
			}
			return true;
		}
	}
}
