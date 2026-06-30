using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class ApexPredatorAugment : Augment
    {
        public override string Id => "apex_predator";
        public override string DisplayName => "Apex Predator";
        public override string Description =>
            $"Damage scales up the lower the target's HP is, up to {AugmentText.BonusDamage("+50% bonus damage")} " +
            $"against an enemy on the brink of death.";

        public override AugmentRarity Rarity => AugmentRarity.Legendary;
        public override AugmentClass Class => AugmentClass.Universal;

        private const float MaxBonusDamagePercent = 0.5f;

        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.lifeMax <= 0)
                return;

            float missingHpPercent = 1f - (target.life / (float)target.lifeMax);
            modifiers.FlatBonusDamage += (int)(item.damage * missingHpPercent * MaxBonusDamagePercent);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.lifeMax <= 0)
                return;

            float missingHpPercent = 1f - (target.life / (float)target.lifeMax);
            modifiers.FlatBonusDamage += (int)(proj.damage * missingHpPercent * MaxBonusDamagePercent);
        }
    }
}
