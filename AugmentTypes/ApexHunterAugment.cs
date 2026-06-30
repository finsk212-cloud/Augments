using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class ApexHunterAugment : Augment
    {
        public override string Id => "apex_hunter";
        public override string DisplayName => "Apex Hunter";
        public override string Description =>
            "Ranged hits against a boss build a mark; once it reaches 10 hits, the next hit triggers a " +
            $"bonus burst dealing {AugmentText.BonusDamage("15% of the boss's max HP")}, then the mark resets.";

        public override AugmentRarity Rarity => AugmentRarity.Legendary;
        public override AugmentClass Class => AugmentClass.Ranged;

        private const int MaxMarkStacks = 10;
        private const float BurstPercentOfMaxHP = 0.15f;

        // Tracked per-player (this augment instance), not per-target - the
        // mark only ever matters against whichever single boss is currently
        // being fought, so there's no need to key it by target.whoAmI.
        private int markStacks;

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (target.boss && item.DamageType == DamageClass.Ranged)
                HandleMark(player, target);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (target.boss && proj.DamageType == DamageClass.Ranged)
                HandleMark(player, target);
        }

        private void HandleMark(Player player, NPC target)
        {
            if (markStacks >= MaxMarkStacks)
            {
                markStacks = 0;
                Strike(player, target);
            }
            else
            {
                markStacks++;
            }
        }

        // Built manually (instead of SimpleStrikeNPC) so the combat text can be
        // forced to yellow - HideCombatText suppresses the auto popup and we
        // spawn our own via CombatText.NewText, same as MeteorStrikeAugment.
        private static void Strike(Player player, NPC target)
        {
            int damage = (int)(target.lifeMax * BurstPercentOfMaxHP);

            var hit = new NPC.HitInfo
            {
                Damage = damage,
                SourceDamage = damage,
                HitDirection = player.direction,
                HideCombatText = true
            };

            target.StrikeNPC(hit);
            CombatText.NewText(target.Hitbox, Color.Yellow, damage);
        }
    }
}
