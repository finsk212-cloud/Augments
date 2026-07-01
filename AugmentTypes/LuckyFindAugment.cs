using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Augments
{
    public class LuckyFindAugment : Augment
    {
        public override string Id => "lucky_find";
        public override string DisplayName => "Lucky Find";
        public override string Description =>
            "Defeated enemies have a 25% chance to drop a small bonus of extra coins. Scales with Fortune.\n" +
            $"Total extra coins gained: {FormatCoins(copperGained)}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        public override bool IsLuckyThemed => true;

        private const float DropChance = 0.25f;
        private const int MinCoins = 1;
        private const int MaxCoins = 3;

        // Coin values in copper, vanilla style: 100 copper = 1 silver,
        // 100 silver = 1 gold, 100 gold = 1 platinum.
        private const int CopperPerSilver = 100;
        private const int CopperPerGold = CopperPerSilver * 100;
        private const int CopperPerPlatinum = CopperPerGold * 100;

        // Tracked in copper so it can roll up cleanly into silver/gold/platinum
        // as it grows, instead of just being a raw count of dropped coin items.
        private int copperGained;

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

        // Called by AugmentBonusCoinItem once the dropped coin is actually
        // picked up - the running total should reflect coins in hand, not
        // coins that are merely on the ground.
        public void CreditCoins(int silverCoinCount)
        {
            copperGained += silverCoinCount * CopperPerSilver;
        }

        // Shows only the highest denomination plus the one below it (e.g.
        // "2 Gold 30 Silver"), and never shows Copper once there's any Silver
        // or Gold - matching how vanilla's own coin totals read at a glance.
        private static string FormatCoins(int copper)
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

        public override void SaveCustomData(TagCompound tag)
        {
            tag["copperGained"] = copperGained;
        }

        public override void LoadCustomData(TagCompound tag)
        {
            copperGained = tag.GetInt("copperGained");
        }
    }
}
