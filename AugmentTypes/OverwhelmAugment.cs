using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class OverwhelmAugment : Augment
    {
        public override string Id => "overwhelm";
        public override string DisplayName => "Overwhelm";
        public override string Description =>
            $"Hitting the same enemy {AugmentText.Trigger("3 times")} within {AugmentText.Duration("1.5s")} applies " +
            $"{AugmentText.Immobilize("Confused")} for {AugmentText.Duration("1s")}. Switching targets or waiting too long resets the count.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Melee;

        private const int ResetWindowTicks = 90;
        private const int ConfusedDurationTicks = 60;
        private const int HitsToTrigger = 3;

        private int lastTargetWhoAmI = -1;
        private int hitCounter;
        private int resetTimer;

        public override void OnUpdate(Player player)
        {
            if (resetTimer <= 0)
                return;

            resetTimer--;
            if (resetTimer == 0)
            {
                hitCounter = 0;
                lastTargetWhoAmI = -1;
            }
        }

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (item.DamageType == DamageClass.Melee)
                RegisterHit(target);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (proj.DamageType == DamageClass.Melee)
                RegisterHit(target);
        }

        private void RegisterHit(NPC target)
        {
            if (target.whoAmI == lastTargetWhoAmI && resetTimer > 0)
                hitCounter++;
            else
            {
                hitCounter = 1;
                lastTargetWhoAmI = target.whoAmI;
            }

            resetTimer = ResetWindowTicks;

            if (hitCounter % HitsToTrigger == 0)
                target.AddBuff(BuffID.Confused, ConfusedDurationTicks);
        }
    }
}
