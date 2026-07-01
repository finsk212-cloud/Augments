using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class HeartDropAugment : Augment
    {
        public override string Id => "heart_drop";
        public override string DisplayName => "Heart Drop";
        public override string Description =>
            $"While {AugmentText.HP("missing HP")}, enemy kills have {AugmentText.Healing("+10% heart drop chance")}.\n" +
            AugmentText.Color("Vanilla Terraria chance: 8.33%. This augment is additive.", "808080");

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const float ExtraHeartDropChance = 0.10f;

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (target.life <= 0)
                TryDropHeart(player, target, HitEffectiveness);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (target.life <= 0)
                TryDropHeart(player, target, HitEffectiveness);
        }

        private static void TryDropHeart(Player player, NPC target, float effectiveness)
        {
            if (target.friendly || player.statLife >= player.statLifeMax2 ||
                !player.GetModPlayer<AugmentPlayer>().HasAugment("heart_drop"))
                return;

            if (Main.rand.NextFloat() < ExtraHeartDropChance * effectiveness)
            {
                Item.NewItem(target.GetSource_Loot(), target.Hitbox, ItemID.Heart);
            }
        }
    }
}
