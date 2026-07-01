using Terraria;

namespace Augments
{
    public class GetExcitedAugment : Augment
    {
        public override string Id => "get_excited";
        public override string DisplayName => "Get Excited!";
        public override string Description =>
            $"After killing an enemy, gain {AugmentText.SpecialDamage("+8% movement speed")} for {AugmentText.Duration("2s")}, " +
            $"stacking up to {AugmentText.SpecialDamage("+24% movement speed")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int DurationTicks = 120;
        private const int MaxStacks = 3;
        private const float MovementSpeedPerStack = 0.08f;

        private int timer;
        private int stacks;

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (target.life <= 0 && PassesHitEffectivenessRoll())
                TriggerEffect();
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (target.life <= 0 && PassesHitEffectivenessRoll())
                TriggerEffect();
        }

        public override void OnUpdate(Player player)
        {
            if (timer <= 0)
                return;

            timer--;

            if (timer == 0)
                stacks = 0;
        }

        public override void PostUpdateRunSpeeds(Player player)
        {
            if (timer <= 0)
                return;

            float bonus = MovementSpeedPerStack * stacks;
            player.maxRunSpeed *= 1f + bonus;
            player.accRunSpeed *= 1f + bonus;
            player.runAcceleration *= 1f + bonus;
        }

        private void TriggerEffect()
        {
            if (stacks < MaxStacks)
                stacks++;

            timer = DurationTicks;
        }
    }
}
