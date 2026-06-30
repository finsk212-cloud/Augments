using Terraria;

namespace Augments
{
    public class VitalEchoAugment : Augment
    {
        public override string Id => "vital_echo";
        public override string DisplayName => "Vital Echo";
        public override string Description =>
            $"Being {AugmentText.Healing("healed")} by any source grants {AugmentText.SpecialDamage("+6 defense")} " +
            $"for {AugmentText.Duration("3 seconds")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int DurationTicks = 180;
        private const int DefenseBonus = 6;

        private int lastLife = -1;
        private int defenseTicksRemaining;

        // Comparing statLife to its value last tick is the universal way to
        // catch ANY healing source (potion, Second Wind, natural regen,
        // anything) without needing a separate hook per source - an increase
        // from one tick to the next always means healing happened, no matter
        // what caused it.
        public override void OnUpdate(Player player)
        {
            if (lastLife != -1 && player.statLife > lastLife)
                defenseTicksRemaining = DurationTicks;

            lastLife = player.statLife;

            if (defenseTicksRemaining > 0)
                defenseTicksRemaining--;
        }

        public override void UpdateEquips(Player player)
        {
            if (defenseTicksRemaining > 0)
                player.statDefense += DefenseBonus;
        }
    }
}
