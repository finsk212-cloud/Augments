using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class OverchargeRoundAugment : Augment
    {
        public override string Id => "overcharge_round";
        public override string DisplayName => "Overcharge Round";
        public override string Description =>
            $"Consecutive ranged hits build a stacking {AugmentText.Trigger("on-hit")} proc, +1 stack per hit up to 10, " +
            $"resetting after {AugmentText.Duration("1 second")} without a hit. The proc deals {AugmentText.SpecialDamage("5")} " +
            $"plus {AugmentText.SpecialDamage("3")} per stack, up to {AugmentText.SpecialDamage("35")} at full charge.";

        public override AugmentRarity Rarity => AugmentRarity.Epic;
        public override AugmentClass Class => AugmentClass.Ranged;

        private const int MaxStacks = 10;
        private const int ResetWindowTicks = 60;
        private const int BaseDamage = 5;
        private const int DamagePerStack = 3;

        private int hitStacks;
        private int resetTimer;

        public override void OnUpdate(Player player)
        {
            if (resetTimer <= 0)
                return;

            resetTimer--;
            if (resetTimer == 0)
                hitStacks = 0;
        }

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (item.DamageType == DamageClass.Ranged)
                ApplyStack(player, target);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (proj.DamageType == DamageClass.Ranged)
                ApplyStack(player, target);
        }

        private void ApplyStack(Player player, NPC target)
        {
            if (resetTimer > 0)
            {
                if (hitStacks < MaxStacks)
                    hitStacks++;
            }
            else
            {
                hitStacks = 1;
            }

            resetTimer = ResetWindowTicks;

            Strike(player, target, BaseDamage + hitStacks * DamagePerStack);
        }

        // Built manually (instead of plain SimpleStrikeNPC) so the combat
        // text can be forced to the same blue used for Iron Rhythm/Twin
        // Strike's popups - HideCombatText suppresses the auto popup and
        // CombatText.NewText spawns our own.
        private static void Strike(Player player, NPC target, int damage)
        {
            var hit = new NPC.HitInfo
            {
                Damage = damage,
                SourceDamage = damage,
                HitDirection = player.direction,
                HideCombatText = true
            };

            target.StrikeNPC(hit);
            CombatText.NewText(target.Hitbox, AugmentTextColors.SpecialDamage, damage);
        }
    }
}
