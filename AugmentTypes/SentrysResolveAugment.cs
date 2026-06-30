using Terraria;

namespace Augments
{
    public class SentrysResolveAugment : Augment
    {
        public override string Id => "sentrys_resolve";
        public override string DisplayName => "Sentry's Resolve";
        public override string Description =>
            $"Sentries last {AugmentText.Duration("30% longer")} before expiring.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Summon;

        private const float DurationMultiplier = 1.3f;

        // projectile.sentry is the only sentry flag that exists - confirmed
        // there's no NPC-based sentry mechanism at all (NPC has zero fields
        // referencing "sentry"), so this single check covers every sentry
        // type (Flameburst Tower, Ballista, Explosive Trap, Lightning Aura,
        // etc) with no per-type special-casing needed.
        //
        // Targeting range is NOT modified - confirmed there's no shared
        // field for it either (no "range" field exists anywhere on
        // Projectile or NPC). Each sentry computes its own targeting range
        // internally with no exposed hook to scale it, so that part is
        // intentionally scoped out rather than faked. Duration (timeLeft)
        // is set once here at spawn, not re-applied every tick, since
        // timeLeft already counts down on its own afterward.
        public override void OnProjectileSpawn(Player player, Projectile projectile)
        {
            if (projectile.sentry)
                projectile.timeLeft = (int)(projectile.timeLeft * DurationMultiplier);
        }
    }
}
