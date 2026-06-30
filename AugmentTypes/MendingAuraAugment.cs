using Terraria;

namespace Augments
{
    public class MendingAuraAugment : Augment
    {
        public override string Id => "mending_aura";
        public override string DisplayName => "Mending Aura";
        public override string Description =>
            $"While standing still, heal {AugmentText.Healing("5 HP")} every second.";

        public override AugmentRarity Rarity => AugmentRarity.Epic;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int HealIntervalTicks = 60;
        private const int HealAmount = 5;

        private int healTimer;

        // Reuses Ambush's exact standing-still check. No nearby-enemy
        // condition - this ticks regardless of what's around.
        public override void OnUpdate(Player player)
        {
            bool isStill = player.velocity.LengthSquared() < 0.01f;

            if (!isStill)
            {
                healTimer = 0;
                return;
            }

            healTimer++;
            if (healTimer < HealIntervalTicks)
                return;

            healTimer = 0;

            if (player.statLife >= player.statLifeMax2)
                return;

            player.statLife = System.Math.Min(player.statLife + HealAmount, player.statLifeMax2);
            player.HealEffect(HealAmount);
        }
    }
}
