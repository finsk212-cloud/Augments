using Terraria;

namespace Augments
{
    public class MirrorImageAugment : Augment
    {
        public override string Id => "mirror_image";
        public override string DisplayName => "Mirror Image";
        public override string Description =>
            $"Incoming hits have a 15% chance to be completely avoided, taking zero damage, " +
            $"followed by a brief {AugmentText.Duration("1.33s")} window of invincibility. Applies to any damage source.";

        public override AugmentRarity Rarity => AugmentRarity.Epic;
        public override AugmentClass Class => AugmentClass.Magic;

        private const float DodgeChance = 0.15f;
        private const int InvulnerabilityTicks = 80;

        // FreeDodge negates the hit itself, but its own follow-up invuln
        // window isn't reliable - same lesson as the earlier Cancel() bug.
        // Manually drive the invuln window instead, the same way
        // PhoenixHeartAugment does: re-force player.immune/immuneTime every
        // tick for the full duration rather than setting it once and
        // trusting it to survive.
        public override bool FreeDodge(Player player, Player.HurtInfo info)
        {
            bool result = Main.rand.NextFloat() < DodgeChance;

            if (result)
                player.GetModPlayer<AugmentPlayer>().MirrorImageInvulnTicks = InvulnerabilityTicks;

            return result;
        }

        public override void OnUpdate(Player player)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();
            if (ap.MirrorImageInvulnTicks > 0)
            {
                player.immune = true;
                player.immuneTime = ap.MirrorImageInvulnTicks;
                ap.MirrorImageInvulnTicks--;
            }
        }
    }
}
