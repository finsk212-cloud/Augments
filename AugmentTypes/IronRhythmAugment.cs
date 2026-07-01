using System;
using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class IronRhythmAugment : Augment
    {
        public override string Id => "iron_rhythm";
        public override string DisplayName => "Iron Rhythm";
        public override string Description =>
            $"Every 4th attack applies {AugmentText.SpecialDamage("15 damage")} {AugmentText.Trigger("on-hit")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int HitsRequired = 4;
        private const int BonusDamage = 15;

        private float hitCounter;
        private int pendingSpecialDamage;

        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            TryApplyBonus(ref modifiers, 1f);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            TryApplyBonus(ref modifiers, 1f);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers, AugmentHitSource source, float effectiveness)
        {
            TryApplyBonus(ref modifiers, effectiveness);
        }

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            TryShowSpecialDamageText(target);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            TryShowSpecialDamageText(target);
        }

        private void TryApplyBonus(ref NPC.HitModifiers modifiers, float effectiveness)
        {
            hitCounter += effectiveness;

            if (hitCounter >= HitsRequired)
            {
                hitCounter -= HitsRequired;
                pendingSpecialDamage = Math.Max(1, (int)(BonusDamage * effectiveness));
                modifiers.FlatBonusDamage += pendingSpecialDamage;
            }
        }

        private void TryShowSpecialDamageText(NPC target)
        {
            if (pendingSpecialDamage <= 0)
                return;

            CombatText.NewText(
                target.Hitbox,
                AugmentTextColors.SpecialDamage,
                $"{pendingSpecialDamage}",
                true
            );

            pendingSpecialDamage = 0;
        }
    }
}
