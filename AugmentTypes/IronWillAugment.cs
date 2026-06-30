using Terraria;

namespace Augments
{
    public class IronWillAugment : Augment
    {
        public override string Id => "iron_will";
        public override string DisplayName => "Iron Will";
        public override string Description =>
            $"After you're hit, gain {AugmentText.SpecialDamage("+8 defense")} for {AugmentText.Duration("3s")}. " +
            $"{AugmentText.Cooldown("30s cooldown")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int DurationTicks = 180;
        private const int CooldownTicks = 1800;
        private const int DefenseBonus = 8;

        private int durationRemaining;
        private int cooldownRemaining;

        public override int CooldownRemaining => cooldownRemaining;

        public override void OnHurt(Player player, Player.HurtInfo info)
        {
            if (cooldownRemaining > 0)
                return;

            durationRemaining = DurationTicks;
            cooldownRemaining = CooldownTicks;
        }

        public override void OnUpdate(Player player)
        {
            if (cooldownRemaining > 0)
                cooldownRemaining--;

            if (durationRemaining > 0)
                durationRemaining--;
        }

        public override void UpdateEquips(Player player)
        {
            if (durationRemaining > 0)
                player.statDefense += DefenseBonus;
        }
    }
}
