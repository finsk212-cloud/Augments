using Terraria;

namespace Augments
{
    public class VoidStepAugment : Augment
    {
        public override string Id => "void_step";
        public override string DisplayName => "Void Step";
        public override string Description =>
            $"Each kill grants +2% dodge chance, stacking up to +20% at 10 stacks, decaying back to 0 if " +
            $"{AugmentText.Duration("4 seconds")} pass without a kill. Stacks independently alongside any " +
            "other dodge-chance augment.";

        public override AugmentRarity Rarity => AugmentRarity.Epic;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int MaxStacks = 10;
        private const float DodgeChancePerStack = 0.02f;
        private const int ResetWindowTicks = 240;
        private const int InvulnerabilityTicks = 80;

        // Shows "+X%" in the cooldown/status row while stacks are active,
        // same StatusValue mechanism ScavengersLuckAugment uses for its crit
        // buff - no dedicated "dodge" color category exists yet, so this
        // stays the default white rather than adding one unasked.
        // Round rather than truncate - float imprecision on DodgeChancePerStack
        // (0.02f isn't exact in binary) otherwise lands at 10 stacks as
        // 19.999998 instead of 20, and (int) truncation would display "19%".
        public override int? StatusValue => LocalPlayerState.VoidStepKillStacks > 0 ? (int)System.Math.Round(LocalPlayerState.VoidStepKillStacks * DodgeChancePerStack * 100f) : (int?)null;
        public override string StatusValueSuffix => "%";

        // Hooking kill credit (see AugmentGlobalNPC.OnKill, keyed off
        // npc.lastInteraction) instead of a hit-based check - this catches
        // kills finished off by a DoT tick too, same shared system
        // SwarmTacticsAugment/PlagueBearerAugment rely on. No DamageType
        // restriction here, so any kill builds a stack.
        public override void OnKillNPC(Player player, NPC npc)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();
            if (ap.VoidStepKillStacks < MaxStacks)
                ap.VoidStepKillStacks++;

            ap.VoidStepResetTimer = ResetWindowTicks;
        }

        public override void OnUpdate(Player player)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();
            if (ap.VoidStepResetTimer > 0)
            {
                ap.VoidStepResetTimer--;
                if (ap.VoidStepResetTimer == 0)
                    ap.VoidStepKillStacks = 0;
            }

            // FreeDodge negates the hit itself, but its own follow-up invuln
            // window isn't reliable - same lesson MirrorImageAugment already
            // learned. Manually drive the invuln window instead: re-force
            // player.immune/immuneTime every tick for the full duration
            // rather than setting it once and trusting it to survive.
            if (ap.VoidStepInvulnTicks > 0)
            {
                player.immune = true;
                player.immuneTime = ap.VoidStepInvulnTicks;
                ap.VoidStepInvulnTicks--;
            }
        }

        public override bool FreeDodge(Player player, Player.HurtInfo info)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();
            bool result = Main.rand.NextFloat() < ap.VoidStepKillStacks * DodgeChancePerStack;

            if (result)
                ap.VoidStepInvulnTicks = InvulnerabilityTicks;

            return result;
        }
    }
}
