using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class IroncladWillAugment : Augment
    {
        public override string Id => "ironclad_will";
        public override string DisplayName => "Ironclad Will";
        public override string Description =>
            "Melee hits have a 20% chance to completely bypass the enemy's defense for that hit, " +
            "dealing full damage regardless of how armored they are.";

        public override AugmentRarity Rarity => AugmentRarity.Legendary;
        public override AugmentClass Class => AugmentClass.Melee;

        private const float ProcChance = 0.2f;

        // ScalingArmorPenetration is a fraction of the target's defense to
        // ignore - at 1f, the hit completely ignores all of it. This is a
        // genuinely different mechanism from Ichor (which lowers Defense
        // itself, a reduction other hits also benefit from) - this only
        // bypasses defense for the one hit that procs it.
        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (item.DamageType == DamageClass.Melee && Main.rand.NextFloat() < ProcChance)
                modifiers.ScalingArmorPenetration += 1f;
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.DamageType == DamageClass.Melee && Main.rand.NextFloat() < ProcChance)
                modifiers.ScalingArmorPenetration += 1f;
        }
    }
}
