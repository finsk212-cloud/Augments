using Terraria;

namespace Augments
{
    public class PotionRushAugment : Augment
    {
        public override string Id => "potion_rush";
        public override string DisplayName => "Potion Rush";
        public override string Description =>
            $"After using a healing potion, gain {AugmentText.SpecialDamage("+10% movement speed")} for {AugmentText.Duration("4s")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int DurationTicks = 240;

        private int timer;

        public override void OnConsumeItem(Player player, Item item)
        {
            if (item.healLife > 0)
            {
                timer = DurationTicks;
            }
        }

        public override void OnUpdate(Player player)
        {
            if (timer <= 0)
                return;

            timer--;
        }

        public override void PostUpdateRunSpeeds(Player player)
        {
            if (timer <= 0)
                return;

            player.maxRunSpeed *= 1.10f;
            player.accRunSpeed *= 1.10f;
            player.runAcceleration *= 1.10f;
        }
    }
}
