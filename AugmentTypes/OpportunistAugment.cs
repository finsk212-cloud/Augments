using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class OpportunistAugment : Augment
    {
        public override string Id => "opportunist";
        public override string DisplayName => "Opportunist";
        public override string Description =>
            $"Melee hits against an airborne enemy (knocked into the air or naturally flying) deal " +
            $"{AugmentText.BonusDamage("+20% bonus damage")}, based on the weapon's base damage.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Melee;

        private const float BonusDamagePercent = 0.20f;

        // noGravity catches naturally flying enemies (Demon Eyes, Harpies,
        // etc.), which otherwise have no reliable velocity.Y signal.
        // velocity.Y != 0f catches everything else mid-air - gravity-affected
        // NPCs sit at exactly 0 while grounded, the same check vanilla AI
        // itself uses to know when an NPC has landed.
        private static bool IsAirborne(NPC target) => target.noGravity || target.velocity.Y != 0f;

        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (item.DamageType == DamageClass.Melee && IsAirborne(target))
                modifiers.FlatBonusDamage += (int)(item.damage * BonusDamagePercent);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.DamageType == DamageClass.Melee && IsAirborne(target))
                modifiers.FlatBonusDamage += (int)(proj.damage * BonusDamagePercent);
        }
    }
}
