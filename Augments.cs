using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
	internal enum AugmentPacketType : byte
	{
		SoulLinkHeal,
		RevitalizingWaveHeal,
		CleanseTrigger,           // client → server: relay CleanseApply to target
		CleanseApply,             // server → target client: clear own debuffs
		BossDamageParticipation,  // client → server: "I damaged boss type X this fight"
		OpenRewardChoices,
		ChooseReward,
		SyncAugmentState,
		RequestAugmentSync,
		DebugRequestReward,
		DebugCommandRequest,
		VendorSellRequest,
		VendorBuyBackRequest,
		RequestVendorSpawn
	}

	internal enum DebugAugmentCommandType : byte
	{
		Add,
		Remove,
		AddAll,
		Clear,
		Sell,
		BuyBack
	}

	public class Augments : Mod
	{
		public static ModKeybind OpenAugmentListKeybind;
		public static ModKeybind DebugTriggerPopupKeybind;
		public static ModKeybind DebugSpawnVendorKeybind;
		public static ModKeybind DebugToggleShopKeybind;
		public static ModKeybind CleanseKeybind;

		public override void Load()
		{
			AugmentDatabase.Load();
			OpenAugmentListKeybind = KeybindLoader.RegisterKeybind(this, "OpenAugmentList", "OemOpenBrackets");
			DebugTriggerPopupKeybind = KeybindLoader.RegisterKeybind(this, "DebugTriggerPopup", "OemCloseBrackets");
			DebugSpawnVendorKeybind = KeybindLoader.RegisterKeybind(this, "DebugSpawnVendor", "OemSemicolon");
			DebugToggleShopKeybind = KeybindLoader.RegisterKeybind(this, "DebugToggleShop", "OemQuotes");
			CleanseKeybind = KeybindLoader.RegisterKeybind(this, "Cleanse", "None");

			// Registered manually, in this exact order, instead of relying on
			// autoload - AugmentFesteringWoundsNPC's UpdateLifeRegen must run
			// AFTER AugmentBleedNPC's so it amplifies a bleed contribution
			// that already landed this same tick. See the comments on both
			// classes for why autoload order can't be trusted for this.
			AddContent(new AugmentBleedNPC());
			AddContent(new AugmentFesteringWoundsNPC());
		}

		public override void Unload()
		{
			OpenAugmentListKeybind = null;
			DebugTriggerPopupKeybind = null;
			DebugSpawnVendorKeybind = null;
			DebugToggleShopKeybind = null;
			CleanseKeybind = null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			var type = (AugmentPacketType)reader.ReadByte();
			if (AugmentNet.HandlePacket(type, reader, whoAmI))
				return;

			switch (type)
			{
				case AugmentPacketType.SoulLinkHeal:
					byte slTarget = reader.ReadByte();
					int healAmount = reader.ReadInt32();
					// Player.Heal() calls NetMessage.SendData(MessageID.PlayerHp) internally
					// when running on the server, syncing the HP change to all clients.
					Main.player[slTarget].Heal(healAmount);
					break;

				case AugmentPacketType.RevitalizingWaveHeal:
					byte rwTarget = reader.ReadByte();
					int rwHeal = reader.ReadInt32();
					Main.player[rwTarget].Heal(rwHeal);
					break;

				case AugmentPacketType.CleanseTrigger:
					// Server receives: relay CleanseApply to the target client.
					if (Main.netMode == NetmodeID.Server)
					{
						byte targetIdx = reader.ReadByte();
						ModPacket relay = GetPacket();
						relay.Write((byte)AugmentPacketType.CleanseApply);
						relay.Send(toClient: targetIdx);
					}
					break;

				case AugmentPacketType.CleanseApply:
					// Target client receives: clear own debuffs.
					CleanseAugment.ClearDebuffs(Main.LocalPlayer);
					break;

				case AugmentPacketType.BossDamageParticipation:
					if (Main.netMode == NetmodeID.Server)
					{
						reader.ReadByte();
						int bossNpcType = reader.ReadInt32();
						Player p = Main.player[whoAmI];
						if (p.active)
							p.GetModPlayer<AugmentPlayer>().DamagedBossesThisFight.Add(bossNpcType);
					}
					break;
			}
		}
	}
}
