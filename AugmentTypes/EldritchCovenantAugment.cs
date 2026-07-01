using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class EldritchCovenantAugment : Augment
    {
        public override string Id => "eldritch_covenant";
        public override string DisplayName => "Eldritch Covenant";
        public override string Description =>
            $"Spending {AugmentText.Mana("mana")} builds corruption. At full corruption, your next magic hit opens an eldritch eye for {AugmentText.Duration("4s")}.";

        public override AugmentRarity Rarity => AugmentRarity.Legendary;
        public override AugmentClass Class => AugmentClass.Magic;
        public override bool IsCharging => corruption > 0 || manaProgress > 0;
        public override int ChargeIndicatorPercent => corruption * 10 + manaProgress;
        public override Texture2D Icon => TextureAssets.Projectile[ProjectileID.PhantasmalEye].Value;

        private const int ManaPerCorruption = 10;
        private const int MaxCorruption = 10;
        private const int LaserDamage = 40;

        private int corruption;
        private int manaProgress;

        public override void OnConsumeMana(Player player, Item item, int manaConsumed)
        {
            if (corruption >= MaxCorruption || manaConsumed <= 0)
                return;

            manaProgress += manaConsumed;
            int corruptionGained = manaProgress / ManaPerCorruption;
            manaProgress %= ManaPerCorruption;
            corruption = Math.Min(MaxCorruption, corruption + corruptionGained);

            if (corruption >= MaxCorruption)
                manaProgress = 0;
        }

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (item.DamageType == DamageClass.Magic)
                TryOpenEye(player, target);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (proj.DamageType == DamageClass.Magic)
                TryOpenEye(player, target);
        }

        private void TryOpenEye(Player player, NPC target)
        {
            if (corruption < MaxCorruption)
                return;

            corruption = 0;
            Projectile.NewProjectile(
                player.GetSource_FromThis(),
                target.Center + new Vector2(0f, -80f),
                Vector2.Zero,
                ModContent.ProjectileType<EldritchEyeProjectile>(),
                LaserDamage,
                0f,
                player.whoAmI
            );
        }
    }
}
