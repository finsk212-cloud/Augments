using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class HuntersPaceAugment : Augment
    {
        public override string Id => "hunters_pace";
        public override string DisplayName => "Hunter's Pace";
        public override string Description =>
            $"Ranged hits give {AugmentText.SpecialDamage("+15% movement speed")} for {AugmentText.Duration("2s")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Ranged;

        private const int SpeedDurationTicks = 120;
        private const float SpeedBonus = 0.15f;

        private int speedTimer;
        private float currentBonus;

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (item.DamageType == DamageClass.Ranged)
                RefreshSpeedBonus();
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (proj.DamageType == DamageClass.Ranged)
                RefreshSpeedBonus();
        }

        public override void OnUpdate(Player player)
        {
            if (speedTimer <= 0)
                return;

            speedTimer--;
            if (speedTimer == 0)
                currentBonus = 0f;
        }

        public override void PostUpdateRunSpeeds(Player player)
        {
            if (speedTimer <= 0)
                return;

            player.maxRunSpeed *= 1f + currentBonus;
            player.accRunSpeed *= 1f + currentBonus;
            player.runAcceleration *= 1f + currentBonus;
        }

        private void RefreshSpeedBonus()
        {
            speedTimer = SpeedDurationTicks;
            currentBonus = SpeedBonus * HitEffectiveness;
        }
    }
}
