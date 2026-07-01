using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class SoulLinkAugment : Augment
    {
        private const float AuraRadius = 600f;

        public override string Id => "soul_link";
        public override string DisplayName => "Soul Link";
        public override string Description =>
            $"Using a healing potion also heals nearby teammates for {AugmentText.Healing("40%")} of the amount healed.";
        public override AugmentRarity Rarity => AugmentRarity.Epic;
        public override AugmentClass Class => AugmentClass.Support;
        public override bool HasAuraEffect => true;

        public override void GetHealLife(Player player, Item item, bool quickHeal, ref int healValue)
        {
            // Works from 1 augment — no stance requirement to trigger the heal.

            int teamHeal = (int)(healValue * 0.40f);
            if (teamHeal <= 0)
                return;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (!other.active || other.dead || other == player)
                    continue;
                if (Microsoft.Xna.Framework.Vector2.Distance(player.Center, other.Center) > AuraRadius)
                    continue;

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    other.Heal(teamHeal);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Server receives this packet, calls Player.Heal() on the target,
                    // which syncs HP to all clients via net message automatically.
                    ModPacket packet = ModLoader.GetMod("Augments").GetPacket();
                    packet.Write((byte)AugmentPacketType.SoulLinkHeal);
                    packet.Write((byte)other.whoAmI);
                    packet.Write(teamHeal);
                    packet.Send();
                }
            }
        }
    }
}
