using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class InfernosHeartAugment : Augment
    {
        public override string Id => "infernos_heart";
        public override string DisplayName => "Inferno's Heart";
        public override string Description =>
            $"8 consecutive magic hits within {AugmentText.Duration("0.5 seconds")} of each other charge a burst; " +
            $"the next hit after reaching full charge releases an AoE explosion dealing " +
            $"{AugmentText.BonusDamage("100 damage")} to every nearby enemy, then the charge resets.";

        public override AugmentRarity Rarity => AugmentRarity.Legendary;
        public override AugmentClass Class => AugmentClass.Magic;

        private const int MaxChargeStacks = 8;
        private const int ResetWindowTicks = 30;
        private const float BurstRange = 200f;
        private const int BurstDamage = 100;

        private int chargeStacks;
        private int resetTimer;

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (item.DamageType == DamageClass.Magic)
                HandleHit(target);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (proj.DamageType == DamageClass.Magic)
                HandleHit(target);
        }

        // Same "next hit after reaching the cap triggers, then resets" shape
        // as ApexHunterAugment - check the threshold from BEFORE this hit
        // first, so the hit that brings the count to 8 just finishes
        // charging, and it's the hit after that releases the burst.
        private void HandleHit(NPC target)
        {
            bool triggers = chargeStacks >= MaxChargeStacks;

            if (triggers)
            {
                chargeStacks = 0;
            }
            else if (resetTimer > 0)
            {
                if (chargeStacks < MaxChargeStacks)
                    chargeStacks++;
            }
            else
            {
                chargeStacks = 1;
            }

            resetTimer = ResetWindowTicks;

            if (triggers)
                Burst(target);
        }

        public override void OnUpdate(Player player)
        {
            if (resetTimer <= 0)
                return;

            resetTimer--;
            if (resetTimer == 0)
                chargeStacks = 0;
        }

        // Uncapped nearby-enemy search - same shape as
        // TimeWarpAugment.SlowNearbyTargets, just dealing damage instead of
        // applying a slow.
        private static void Burst(NPC origin)
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.townNPC)
                    continue;

                if (npc.Distance(origin.Center) <= BurstRange)
                {
                    int direction = npc.Center.X >= origin.Center.X ? 1 : -1;
                    npc.SimpleStrikeNPC(BurstDamage, direction);
                }
            }
        }
    }
}
