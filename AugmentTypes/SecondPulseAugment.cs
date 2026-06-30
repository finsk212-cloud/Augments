using Terraria;

namespace Augments
{
    public class SecondPulseAugment : Augment
    {
        public override string Id => "second_pulse";
        public override string DisplayName => "Second Pulse";
        public override string Description =>
            $"Healing potions restore {AugmentText.Healing("+10 extra HP")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int BonusHealing = 10;

        public override void GetHealLife(Player player, Item item, bool quickHeal, ref int healValue)
        {
            if (item.potion)
                healValue += BonusHealing;
        }
    }
}
