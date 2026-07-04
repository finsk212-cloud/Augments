using System.IO;
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

		// Without this, IsLuckyFindBonus never survives the item's own
		// network sync (NewItem's world-drop broadcast, or any later
		// SyncItem/InstancedItem sync) - the receiving side reconstructs
		// this GlobalItem instance from scratch at its default (false),
		// silently losing the flag before OnPickup ever sees it.
		public override void NetSend(Item item, BinaryWriter writer)
		{
			writer.Write(IsLuckyFindBonus);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			IsLuckyFindBonus = reader.ReadBoolean();
		}

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
