using Terraria;

namespace Augments
{
    public class BulwarkAugment : Augment
    {
        public override string Id => "bulwark";
        public override string DisplayName => "Bulwark";
        public override string Description =>
            $"Going {AugmentText.Duration("10 seconds")} without taking damage charges a shield that completely " +
            "blocks your next hit, then resets and must recharge.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int ChargeTicksRequired = 600;

        public override void OnUpdate(Player player)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();
            if (ap.BulwarkNoDamageTicks < ChargeTicksRequired)
                ap.BulwarkNoDamageTicks++;
        }

        // Unlike MirrorImageAugment, no manual invincibility-window
        // re-assertion is needed here - Bulwark's 10-second recharge already
        // guarantees a long natural gap before it can trigger again, so
        // there's no risk of it firing twice in quick succession.
        public override bool FreeDodge(Player player, Player.HurtInfo info)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();
            if (ap.BulwarkNoDamageTicks < ChargeTicksRequired)
                return false;

            ap.BulwarkNoDamageTicks = 0;
            return true;
        }

        public override void OnHurt(Player player, Player.HurtInfo info)
        {
            player.GetModPlayer<AugmentPlayer>().BulwarkNoDamageTicks = 0;
        }
    }
}
