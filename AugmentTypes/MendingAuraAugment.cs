using Terraria;
using Terraria.ModLoader;

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

        public override void OnUpdate(Player player)
        {
            player.GetModPlayer<AugmentPlayer>().TickMendingAura();
        }
    }
}
