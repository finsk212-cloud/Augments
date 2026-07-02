using Terraria;

namespace Augments
{
    public class GrappleMasterAugment : Augment
    {
        public override string Id => "grapple_master";
        public override string DisplayName => "Grapple Master";
        public override string Description =>
            $"Releasing your grappling hook grants {AugmentText.MovementSpeed("+25% movement speed")} for " +
            $"{AugmentText.Duration("4 seconds")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int DurationTicks = 240;
        private const float SpeedBonus = 0.25f;

        // player.grapCount is reset to 0 during the player's own update phase and
        // re-incremented by grapple projectiles later in the frame, so it reads as 0
        // here inside PostUpdate regardless of actual grapple state. Scanning
        // Main.projectile directly avoids this timing issue - Main.projHook[type]
        // identifies hook-type projectiles across all mods.
        private static bool PlayerHasActiveHook(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == player.whoAmI && Main.projHook[proj.type])
                    return true;
            }
            return false;
        }

        public override void OnUpdate(Player player)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();
            bool isGrappling = PlayerHasActiveHook(player);

            if (ap.GrappleMasterWasGrappling && !isGrappling)
                ap.GrappleMasterSpeedBurstTicks = DurationTicks;

            ap.GrappleMasterWasGrappling = isGrappling;

            if (ap.GrappleMasterSpeedBurstTicks > 0)
                ap.GrappleMasterSpeedBurstTicks--;
        }

        // Same PostUpdateRunSpeeds pattern FightOrFlightAugment uses.
        public override void PostUpdateRunSpeeds(Player player)
        {
            if (player.GetModPlayer<AugmentPlayer>().GrappleMasterSpeedBurstTicks <= 0)
                return;

            player.maxRunSpeed *= 1f + SpeedBonus;
            player.accRunSpeed *= 1f + SpeedBonus;
            player.runAcceleration *= 1f + SpeedBonus;
        }
    }
}
