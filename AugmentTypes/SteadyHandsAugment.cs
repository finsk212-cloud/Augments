using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class SteadyHandsAugment : Augment
    {
        public override string Id => "steady_hands";
        public override string DisplayName => "Steady Hands";
        public override string Description =>
            $"Standing still continuously builds ranged {AugmentText.Crit("crit chance")}, up to +20%, " +
            "ramping over a few seconds. Resets instantly when you move.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Ranged;

        private const int TicksPerPercent = 6;
        private const float MaxCritBonus = 20f;

        private float currentCritBonus;
        private int rampTicks;

        public override bool IsCharging => currentCritBonus > 0;
        public override int ChargeIndicatorPercent => (int)currentCritBonus;

        public override void OnUpdate(Player player)
        {
            bool isStill = player.velocity.LengthSquared() < 0.01f;

            if (!isStill)
            {
                currentCritBonus = 0f;
                rampTicks = 0;
                return;
            }

            if (currentCritBonus >= MaxCritBonus)
                return;

            rampTicks++;
            if (rampTicks >= TicksPerPercent)
            {
                rampTicks = 0;
                currentCritBonus = System.Math.Min(currentCritBonus + 1f, MaxCritBonus);
            }
        }

        public override void ModifyWeaponCrit(Player player, Item item, ref float crit)
        {
            if (item.DamageType == DamageClass.Ranged)
                crit += currentCritBonus;
        }
    }
}
