using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
	internal static class AugmentNet
	{
		// DEBUG COMMANDS - remove or restrict before public release.
		public static readonly bool EnableDebugCommandsInMultiplayer = true;

		private static readonly Dictionary<int, HashSet<string>> PendingRewardChoicesByPlayer = new Dictionary<int, HashSet<string>>();

		public static bool HandlePacket(AugmentPacketType type, BinaryReader reader, int whoAmI)
		{
			switch (type)
			{
				case AugmentPacketType.OpenRewardChoices:
					HandleOpenRewardChoices(reader);
					return true;
				case AugmentPacketType.ChooseReward:
					HandleChooseReward(reader, whoAmI);
					return true;
				case AugmentPacketType.SyncAugmentState:
					HandleSyncAugmentState(reader);
					return true;
				case AugmentPacketType.RequestAugmentSync:
					HandleRequestAugmentSync(whoAmI);
					return true;
				case AugmentPacketType.DebugRequestReward:
					HandleDebugRewardRequest(whoAmI);
					return true;
				case AugmentPacketType.DebugCommandRequest:
					HandleDebugCommandRequest(reader, whoAmI);
					return true;
				case AugmentPacketType.VendorSellRequest:
					HandleVendorSellRequest(reader, whoAmI);
					return true;
				case AugmentPacketType.VendorBuyBackRequest:
					HandleVendorBuyBackRequest(reader, whoAmI);
					return true;
				case AugmentPacketType.RequestVendorSpawn:
					HandleVendorSpawnRequest(whoAmI);
					return true;
			}

			return false;
		}

		public static void SendRewardChoices(int toClient, List<Augment> choices, AugmentRarity rarity)
		{
			if (Main.netMode != NetmodeID.Server)
				return;

			var pendingIds = new HashSet<string>();
			foreach (var augment in choices)
			{
				if (augment != null)
					pendingIds.Add(augment.Id);
			}

			PendingRewardChoicesByPlayer[toClient] = pendingIds;

			ModPacket packet = ModContent.GetInstance<Augments>().GetPacket();
			packet.Write((byte)AugmentPacketType.OpenRewardChoices);
			packet.Write((byte)rarity);
			packet.Write((byte)choices.Count);
			foreach (var augment in choices)
				packet.Write(augment.Id);
			packet.Send(toClient);
		}

		public static void SendChooseReward(string augmentId)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return;

			ModPacket packet = ModContent.GetInstance<Augments>().GetPacket();
			packet.Write((byte)AugmentPacketType.ChooseReward);
			packet.Write(augmentId);
			packet.Send();
		}

		public static void SendSyncPlayer(Player player, int toClient = -1)
		{
			if (Main.netMode != NetmodeID.Server || player == null || !player.active)
				return;

			ModPacket packet = ModContent.GetInstance<Augments>().GetPacket();
			packet.Write((byte)AugmentPacketType.SyncAugmentState);
			packet.Write((byte)player.whoAmI);
			player.GetModPlayer<AugmentPlayer>().WriteAugmentState(packet);
			packet.Send(toClient);
		}

		public static void SendRequestAugmentSync()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return;

			ModPacket packet = ModContent.GetInstance<Augments>().GetPacket();
			packet.Write((byte)AugmentPacketType.RequestAugmentSync);
			packet.Send();
		}

		public static void SendDebugRewardRequest()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return;

			ModPacket packet = ModContent.GetInstance<Augments>().GetPacket();
			packet.Write((byte)AugmentPacketType.DebugRequestReward);
			packet.Send();
		}

		public static void SendDebugCommandRequest(DebugAugmentCommandType command, string augmentId = "")
		{
			if (Main.netMode != NetmodeID.MultiplayerClient || !EnableDebugCommandsInMultiplayer)
				return;

			ModPacket packet = ModContent.GetInstance<Augments>().GetPacket();
			packet.Write((byte)AugmentPacketType.DebugCommandRequest);
			packet.Write((byte)command);
			packet.Write(augmentId ?? "");
			packet.Send();
		}

		public static void SendVendorSellRequest(string augmentId)
		{
			SendVendorRequest(AugmentPacketType.VendorSellRequest, augmentId);
		}

		public static void SendVendorBuyBackRequest(string augmentId)
		{
			SendVendorRequest(AugmentPacketType.VendorBuyBackRequest, augmentId);
		}

		private static void SendVendorRequest(AugmentPacketType type, string augmentId)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient || string.IsNullOrEmpty(augmentId))
				return;

			ModPacket packet = ModContent.GetInstance<Augments>().GetPacket();
			packet.Write((byte)type);
			packet.Write(augmentId);
			packet.Send();
		}

		public static void RequestVendorSpawn(Player player)
		{
			if (player == null || !player.active)
				return;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket packet = ModContent.GetInstance<Augments>().GetPacket();
				packet.Write((byte)AugmentPacketType.RequestVendorSpawn);
				packet.Send();
				return;
			}

			SpawnVendor(player);
		}

		private static void HandleOpenRewardChoices(BinaryReader reader)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return;

			AugmentRarity rarity = (AugmentRarity)reader.ReadByte();
			int count = reader.ReadByte();
			var choices = new List<Augment>();
			for (int i = 0; i < count; i++)
			{
				string id = reader.ReadString();
				Augment augment = AugmentDatabase.GetById(id);
				if (augment != null)
					choices.Add(augment);
			}

			if (choices.Count > 0)
				ModContent.GetInstance<AugmentUISystem>().ShowChoices(choices, rarity, true);
		}

		private static void HandleChooseReward(BinaryReader reader, int whoAmI)
		{
			string augmentId = reader.ReadString();

			if (Main.netMode != NetmodeID.Server)
				return;
			if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
				return;
			if (!PendingRewardChoicesByPlayer.TryGetValue(whoAmI, out var pendingChoices))
				return;
			if (!pendingChoices.Contains(augmentId))
				return;

			Player player = Main.player[whoAmI];
			if (!player.active)
				return;

			var augmentPlayer = player.GetModPlayer<AugmentPlayer>();
			Augment augment = AugmentDatabase.GetById(augmentId);
			if (augment == null || augmentPlayer.HasAugment(augmentId))
				return;

			if (!augmentPlayer.ApplyRewardAugment(augment, false))
				return;

			PendingRewardChoicesByPlayer.Remove(whoAmI);
			SendSyncPlayer(player);
		}

		private static void HandleSyncAugmentState(BinaryReader reader)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
				return;

			int playerIndex = reader.ReadByte();
			if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
				return;

			Main.player[playerIndex].GetModPlayer<AugmentPlayer>().ReadAugmentState(reader);
			if (playerIndex == Main.myPlayer)
				ModContent.GetInstance<AugmentUISystem>().RefreshOpenPlayerPanels();
		}

		private static void HandleRequestAugmentSync(int whoAmI)
		{
			if (Main.netMode != NetmodeID.Server)
				return;
			if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
				return;

			PendingRewardChoicesByPlayer.Remove(whoAmI);

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				if (player.active)
					SendSyncPlayer(player, whoAmI);
			}
		}

		private static void HandleVendorSpawnRequest(int whoAmI)
		{
			if (Main.netMode != NetmodeID.Server || whoAmI < 0 || whoAmI >= Main.maxPlayers)
				return;

			Player player = Main.player[whoAmI];
			if (player.active)
				SpawnVendor(player);
		}

		private static void HandleDebugRewardRequest(int whoAmI)
		{
			if (Main.netMode != NetmodeID.Server || whoAmI < 0 || whoAmI >= Main.maxPlayers)
				return;

			Player player = Main.player[whoAmI];
			if (player.active)
				AugmentRewardLogic.GrantReward(player, RarityBracket.Endgame);
		}

		private static void HandleDebugCommandRequest(BinaryReader reader, int whoAmI)
		{
			DebugAugmentCommandType command = (DebugAugmentCommandType)reader.ReadByte();
			string augmentId = reader.ReadString();

			if (Main.netMode != NetmodeID.Server || !EnableDebugCommandsInMultiplayer)
				return;
			if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
				return;

			Player player = Main.player[whoAmI];
			if (!player.active || !ApplyDebugCommand(player, command, augmentId))
				return;

			SendSyncPlayer(player);
		}

		private static void HandleVendorSellRequest(BinaryReader reader, int whoAmI)
		{
			string augmentId = reader.ReadString();
			if (!TryGetActiveSender(whoAmI, out Player player))
				return;

			AugmentPlayer augmentPlayer = player.GetModPlayer<AugmentPlayer>();
			if (augmentPlayer.SellAugmentByIdServerAuthoritative(augmentId, false))
				SendSyncPlayer(player);
		}

		private static void HandleVendorBuyBackRequest(BinaryReader reader, int whoAmI)
		{
			string augmentId = reader.ReadString();
			if (!TryGetActiveSender(whoAmI, out Player player))
				return;

			AugmentPlayer augmentPlayer = player.GetModPlayer<AugmentPlayer>();
			if (augmentPlayer.BuyBackSoldAugmentByIdServerAuthoritative(augmentId, false))
				SendSyncPlayer(player);
		}

		private static bool TryGetActiveSender(int whoAmI, out Player player)
		{
			player = null;
			if (Main.netMode != NetmodeID.Server || whoAmI < 0 || whoAmI >= Main.maxPlayers)
				return false;

			player = Main.player[whoAmI];
			return player.active;
		}

		public static bool ApplyDebugCommand(Player player, DebugAugmentCommandType command, string augmentId = "")
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || player == null || !player.active)
				return false;

			AugmentPlayer augmentPlayer = player.GetModPlayer<AugmentPlayer>();
			switch (command)
			{
			case DebugAugmentCommandType.Add:
					return augmentPlayer.GrantAugmentByIdServerAuthoritative(augmentId, false);
				case DebugAugmentCommandType.Remove:
					return augmentPlayer.RemoveAugmentByIdServerAuthoritative(augmentId, false);
				case DebugAugmentCommandType.AddAll:
				{
					bool changed = false;
					foreach (Augment augment in AugmentDatabase.All)
					{
						if (augmentPlayer.OwnedIds.Count >= AugmentPlayer.MaxOwnedAugments)
							break;
						if (augmentPlayer.GrantAugmentByIdServerAuthoritative(augment.Id, false))
							changed = true;
					}
					return changed;
				}
				case DebugAugmentCommandType.Clear:
				{
					if (augmentPlayer.OwnedIds.Count == 0)
						return false;

					foreach (string id in new List<string>(augmentPlayer.OwnedIds))
						augmentPlayer.RemoveAugmentByIdServerAuthoritative(id, false);
					return true;
				}
				case DebugAugmentCommandType.Sell:
					return augmentPlayer.SellAugmentByIdServerAuthoritative(augmentId, false);
				case DebugAugmentCommandType.BuyBack:
					return augmentPlayer.BuyBackSoldAugmentByIdServerAuthoritative(augmentId, false);
				default:
					return false;
			}
		}

		private static void SpawnVendor(Player player)
		{
			int vendorType = ModContent.NPCType<AugmentVendorNPC>();
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				if (Main.npc[i].active && Main.npc[i].type == vendorType)
					return;
			}

			int npcIndex = NPC.NewNPC(
				player.GetSource_FromThis(),
				(int)player.Center.X,
				(int)player.Center.Y,
				vendorType);

			if (Main.netMode == NetmodeID.Server && npcIndex >= 0 && npcIndex < Main.maxNPCs)
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcIndex);
		}
	}
}
