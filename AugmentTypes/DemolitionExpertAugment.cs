using Terraria;
using Terraria.ID;

namespace Augments
{
    public class DemolitionExpertAugment : Augment
    {
        public override string Id => "demolition_expert";
        public override string DisplayName => "Demolition Expert";
        public override string Description =>
            $"Explosive hits (bombs, dynamite, rocket launchers, grenade launchers) deal " +
            $"{AugmentText.BonusDamage("+20% bonus damage")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const float BonusDamagePercent = 0.20f;

        // ProjectileID.Sets.Explosive is the verified universal flag shared
        // by every explosive projectile (bombs, dynamite, rocket/grenade
        // launcher shots, etc) - no DamageClass distinction exists for
        // "explosive", and no hardcoded ID list is needed.
        //
        // Blast radius is NOT modified - confirmed there's no shared field
        // (no "radius"/"blast" field exists anywhere on Projectile). Each
        // explosive projectile computes its own explosion radius internally
        // with no exposed hook to scale it, so that part is intentionally
        // scoped out rather than faked.
        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (ProjectileID.Sets.Explosive[proj.type])
                modifiers.FlatBonusDamage += (int)(proj.damage * BonusDamagePercent);
        }
    }
}
