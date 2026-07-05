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
		public long BonusCoinValue;

		// Without this, the tagged coin value never survives the item's own
		// network sync (NewItem's world-drop broadcast, or any later
		// SyncItem/InstancedItem sync) - the receiving side reconstructs
		// this GlobalItem instance from scratch at its default (zero),
		// silently losing the flag before OnPickup ever sees it.
		public override void NetSend(Item item, BinaryWriter writer)
		{
			writer.Write(IsLuckyFindBonus);
			writer.Write(BonusCoinValue);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			IsLuckyFindBonus = reader.ReadBoolean();
			BonusCoinValue = reader.ReadInt64();
		}

		public override bool CanStackInWorld(Item destination, Item source)
		{
			var sourceData = source.GetGlobalItem<AugmentBonusCoinItem>();
			return IsLuckyFindBonus == sourceData.IsLuckyFindBonus;
		}

		public override void OnStack(Item destination, Item source, int numToTransfer)
		{
			var sourceData = source.GetGlobalItem<AugmentBonusCoinItem>();
			if (!sourceData.IsLuckyFindBonus || source.stack <= 0)
				return;

			long valueTransferred = sourceData.BonusCoinValue * numToTransfer / source.stack;
			IsLuckyFindBonus = true;
			BonusCoinValue += valueTransferred;
			sourceData.BonusCoinValue -= valueTransferred;
			sourceData.IsLuckyFindBonus = sourceData.BonusCoinValue > 0;
		}

		public override void SplitStack(Item destination, Item source, int numToTransfer)
		{
			var sourceData = source.GetGlobalItem<AugmentBonusCoinItem>();
			if (!sourceData.IsLuckyFindBonus || source.stack <= 0)
				return;

			long valueTransferred = sourceData.BonusCoinValue * numToTransfer / source.stack;
			IsLuckyFindBonus = true;
			BonusCoinValue = valueTransferred;
			sourceData.BonusCoinValue -= valueTransferred;
			sourceData.IsLuckyFindBonus = sourceData.BonusCoinValue > 0;
		}

		public override bool OnPickup(Item item, Player player)
		{
			if (IsLuckyFindBonus && BonusCoinValue > 0)
			{
				var ap = player.GetModPlayer<AugmentPlayer>();
				ap.LuckyFindCopperGained += BonusCoinValue;
			}
			return true;
		}
	}
}
