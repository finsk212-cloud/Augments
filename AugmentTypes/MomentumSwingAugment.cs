using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class MomentumSwingAugment : Augment
    {
        public override string Id => "momentum_swing";
        public override string DisplayName => "Momentum Swing";
        public override string Description =>
            $"Hitting the same enemy repeatedly within {AugmentText.Duration("1.5s")} builds " +
            $"{AugmentText.BonusDamage("+3% bonus damage")} per stack, up to " +
            $"{AugmentText.BonusDamage("+15% bonus damage")} at 5 stacks. Switching targets or waiting too long resets the stacks.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Melee;

        private const int ResetWindowTicks = 90;
        private const int MaxStacks = 5;
        private const float BonusPerStackPercent = 0.03f;

        private int lastTargetWhoAmI = -1;
        private int stacks;
        private int resetTimer;

        public override void OnUpdate(Player player)
        {
            if (resetTimer <= 0)
                return;

            resetTimer--;
            if (resetTimer == 0)
            {
                stacks = 0;
                lastTargetWhoAmI = -1;
            }
        }

        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (item.CountsAsClass(DamageClass.Melee))
                ApplyStack(ref modifiers, target, item.damage);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.CountsAsClass(DamageClass.Melee))
                ApplyStack(ref modifiers, target, proj.damage);
        }

        private void ApplyStack(ref NPC.HitModifiers modifiers, NPC target, int baseDamage)
        {
            if (target.whoAmI == lastTargetWhoAmI && resetTimer > 0)
            {
                if (stacks < MaxStacks)
                    stacks++;
            }
            else
            {
                stacks = 1;
                lastTargetWhoAmI = target.whoAmI;
            }

            resetTimer = ResetWindowTicks;

            float bonusPercent = stacks * BonusPerStackPercent;
            modifiers.FlatBonusDamage += (int)(baseDamage * bonusPercent);
        }
    }
}
