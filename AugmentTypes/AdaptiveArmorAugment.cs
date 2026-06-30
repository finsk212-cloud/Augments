using Microsoft.Xna.Framework;
using Terraria;

namespace Augments
{
    public class AdaptiveArmorAugment : Augment
    {
        public override string Id => "adaptive_armor";
        public override string DisplayName => "Adaptive Armor";
        public override string Description =>
            $"Defense grows by {AugmentText.SpecialDamage("+1")} every {AugmentText.Duration("2s")} you go without taking damage, " +
            $"up to {AugmentText.SpecialDamage("+10")} after {AugmentText.Duration("20s")}. Resets to zero the instant you're hit.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int TicksPerStack = 120;
        private const int MaxBonus = 10;

        private int undamagedTimer;
        private int defenseBonus;

        public override int? StatusValue => defenseBonus > 0 ? defenseBonus : (int?)null;
        public override Color StatusValueColor => AugmentTextColors.SpecialDamage;

        public override void OnUpdate(Player player)
        {
            if (defenseBonus >= MaxBonus)
                return;

            undamagedTimer++;
            if (undamagedTimer >= TicksPerStack)
            {
                undamagedTimer = 0;
                defenseBonus++;
            }
        }

        public override void OnHurt(Player player, Player.HurtInfo info)
        {
            undamagedTimer = 0;
            defenseBonus = 0;
        }

        public override void UpdateEquips(Player player)
        {
            player.statDefense += defenseBonus;
        }
    }
}
