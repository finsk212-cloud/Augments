using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class LuckyFindAugment : Augment
    {
        public override string Id => "lucky_find";
        public override string DisplayName => "Lucky Find";
        public override string Description =>
            "Defeated enemies have a 25% chance to drop a small bonus of extra coins. Scales with Fortune.\n" +
            $"Total extra coins gained: {FormatCoins(Main.LocalPlayer?.GetModPlayer<AugmentPlayer>()?.LuckyFindCopperGained ?? 0)}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        public override bool IsLuckyThemed => true;

        private const float DropChance = 0.25f;
        private const int MinCoins = 1;
        private const int MaxCoins = 3;

        internal const int CopperPerSilver = 100;
        private const int CopperPerGold = CopperPerSilver * 100;
        private const int CopperPerPlatinum = CopperPerGold * 100;

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (target.life <= 0)
                TryDropCoins(player, target, HitEffectiveness);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (target.life <= 0)
                TryDropCoins(player, target, HitEffectiveness);
        }

        private void TryDropCoins(Player player, NPC target, float effectiveness)
        {
            float chance = DropChance * (1f + player.GetModPlayer<AugmentPlayer>().TotalFortune) * effectiveness;
            if (Main.rand.NextFloat() >= chance)
                return;

            int coinCount = Main.rand.Next(MinCoins, MaxCoins + 1);
            int index = Item.NewItem(target.GetSource_Loot(), target.Hitbox, ItemID.SilverCoin, coinCount);
            Main.item[index].GetGlobalItem<AugmentBonusCoinItem>().IsLuckyFindBonus = true;
        }

        internal static string FormatCoins(int copper)
        {
            int platinum = copper / CopperPerPlatinum;
            int gold = copper % CopperPerPlatinum / CopperPerGold;
            int silver = copper % CopperPerGold / CopperPerSilver;

            if (platinum > 0)
                return $"{platinum} Platinum {gold} Gold";
            if (gold > 0)
                return $"{gold} Gold {silver} Silver";
            if (silver > 0)
                return $"{silver} Silver";

            return $"{copper} Copper";
        }
    }
}
