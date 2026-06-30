using System;
using Terraria;

namespace Augments
{
    public class SecondWindAugment : Augment
    {
        public override string Id => "second_wind";
        public override string DisplayName => "Second Wind";
        public override string Description =>
            $"When you fall below {AugmentText.HP("25% HP")}, {AugmentText.Healing("heal 40 HP")}. " +
            $"{AugmentText.Cooldown("60s cooldown")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int CooldownTicks = 3600;
        private const float TriggerThreshold = 0.25f;
        private const int HealAmount = 40;

        private int cooldownRemaining;

        public override int CooldownRemaining => cooldownRemaining;

        public override void OnUpdate(Player player)
        {
            if (cooldownRemaining > 0)
            {
                cooldownRemaining--;
                return;
            }

            if (player.statLife > 0 && player.statLife < player.statLifeMax2 * TriggerThreshold)
            {
                player.statLife = Math.Min(player.statLife + HealAmount, player.statLifeMax2);
                player.HealEffect(HealAmount);
                cooldownRemaining = CooldownTicks;
            }
        }
    }
}
