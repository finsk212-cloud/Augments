using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
	public class SoulLinkAugment : Augment
	{
		public override string Id => "soul_link";
		public override string DisplayName => "Soul Link";
		public override string Description =>
			$"Using a healing potion also heals nearby teammates for {AugmentText.Healing("40%")} of the amount healed.";
		public override AugmentRarity Rarity => AugmentRarity.Epic;
		public override AugmentClass Class => AugmentClass.Support;
		public override bool HasAuraEffect => true;

		public override void OnConsumeItem(Player player, Item item)
		{
			if (item.healLife <= 0)
				return;

			int teamHeal = (int)(item.healLife * 0.40f);
			if (teamHeal <= 0)
				return;

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				// Covers both SinglePlayer and Server (a hosting player's own
				// client also runs as Server, not SinglePlayer, in "Host & Play"
				// multiplayer - a narrower SinglePlayer-only check here would
				// silently never heal teammates for the host).
				foreach (Player target in Main.player)
				{
					if (SupportEffects.IsAllyInRange(player, target, SupportEffects.AuraRadius))
						SupportEffects.ServerHealPlayer(target, teamHeal);
				}
			}
			else if (player.whoAmI == Main.myPlayer)
			{
				ModPacket packet = ModContent.GetInstance<Augments>().GetPacket();
				packet.Write((byte)AugmentPacketType.SoulLinkRequest);
				packet.Write(teamHeal);
				packet.Send();
			}
		}
	}
}
