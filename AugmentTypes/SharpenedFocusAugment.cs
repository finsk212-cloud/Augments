using Terraria;

namespace Augments
{
    public class SharpenedFocusAugment : Augment
    {
        public override string Id => "sharpened_focus";
        public override string DisplayName => "Sharpened Focus";
        public override string Description =>
            $"All weapons gain {AugmentText.Crit("+40% crit chance")}.";
        public override AugmentRarity Rarity => AugmentRarity.Epic;
        public override AugmentClass Class => AugmentClass.Universal;

        public override void ModifyWeaponCrit(Player player, Item item, ref float crit)
        {
            crit += 40f;
        }
    }
}
