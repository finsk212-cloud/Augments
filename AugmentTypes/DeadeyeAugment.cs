using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class DeadeyeAugment : Augment
    {
        public override string Id => "deadeye";
        public override string DisplayName => "Deadeye";
        public override string Description =>
            $"Consecutive ranged hits on the same enemy build {AugmentText.Crit("+2% crit chance")} per hit, " +
            $"up to {AugmentText.Crit("+16% crit chance")} at 8 stacks. Switching targets resets the stacks.";

        public override AugmentRarity Rarity => AugmentRarity.Epic;
        public override AugmentClass Class => AugmentClass.Ranged;

        private const int MaxStacks = 8;
        private const float CritPerStack = 2f;

        private int lastTargetWhoAmI = -1;
        private float hitStacks;

        // Shows "+X%" in the cooldown/status row while stacks are active,
        // same StatusValue mechanism ScavengersLuckAugment uses for its crit
        // buff, reusing the existing Crit color category.
        public override int? StatusValue => hitStacks > 0 ? (int)(hitStacks * CritPerStack) : (int?)null;
        public override Color StatusValueColor => AugmentTextColors.Crit;
        public override string StatusValueSuffix => "%";

        // Unlike MomentumSwingAugment, there's no time-based reset window
        // here - only switching targets resets the stacks. A missed shot
        // doesn't reduce hitStacks on its own; it simply means this method
        // never fires for that shot, leaving the stacks untouched.
        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (item.DamageType == DamageClass.Ranged)
                ApplyStack(target);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (proj.DamageType == DamageClass.Ranged)
                ApplyStack(target);
        }

        private void ApplyStack(NPC target)
        {
            if (target.whoAmI == lastTargetWhoAmI)
            {
                if (hitStacks < MaxStacks)
                    hitStacks += HitEffectiveness;
            }
            else
            {
                hitStacks = HitEffectiveness;
                lastTargetWhoAmI = target.whoAmI;
            }
        }

        public override void ModifyWeaponCrit(Player player, Item item, ref float crit)
        {
            if (item.DamageType == DamageClass.Ranged)
                crit += hitStacks * CritPerStack;
        }
    }
}
