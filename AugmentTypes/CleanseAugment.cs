using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class CleanseAugment : Augment
    {
        private const float AuraRadius = 600f;
        private const int CooldownTicks = 1800; // 30 seconds

        public override string Id => "cleanse";
        public override string DisplayName => "Cleanse";
        public override string Description =>
            $"{AugmentText.Active("Active:")} Press [Cleanse] to remove all debuffs from nearby teammates. {AugmentText.Cooldown("30s cooldown")}.";
        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Support;
        public override bool HasAuraEffect => true;
        public override ModKeybind ActiveModKeybind => Augments.CleanseKeybind;

        private int cleanseCooldown;

        public override int CooldownRemaining => cleanseCooldown;

        public override void OnUpdate(Player player)
        {
            if (cleanseCooldown > 0)
            {
                cleanseCooldown--;
                return;
            }

            if (Augments.CleanseKeybind == null || !Augments.CleanseKeybind.JustPressed) return;

            cleanseCooldown = CooldownTicks;
            SoundEngine.PlaySound(SoundID.Item4, player.Center);

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (!other.active || other.dead || other == player) continue;
                if (Microsoft.Xna.Framework.Vector2.Distance(player.Center, other.Center) > AuraRadius) continue;

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    ClearDebuffs(other);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Server relays CleanseApply to the target client (see HandlePacket).
                    ModPacket packet = ModLoader.GetMod("Augments").GetPacket();
                    packet.Write((byte)AugmentPacketType.CleanseTrigger);
                    packet.Write((byte)other.whoAmI);
                    packet.Send();
                }
            }
        }

        // Called by CleanseApply packet on the target client (and directly in singleplayer).
        public static void ClearDebuffs(Player player)
        {
            for (int j = player.buffType.Length - 1; j >= 0; j--)
            {
                if (player.buffType[j] > 0 && Main.debuff[player.buffType[j]])
                    player.DelBuff(j);
            }
        }
    }
}
