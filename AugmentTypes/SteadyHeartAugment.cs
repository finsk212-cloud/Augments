using Terraria;

namespace Augments
{
    public class SteadyHeartAugment : Augment
    {
        public override string Id => "steady_heart";
        public override string DisplayName => "Steady Heart";
        public override string Description =>
            $"While above {AugmentText.HP("80% HP")}, gain {AugmentText.BonusDamage("+5% damage")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const float HealthThreshold = 0.8f;
        private const float DamageMultiplier = 1.05f;

        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            ApplyDamageBonus(player, ref modifiers);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            ApplyDamageBonus(player, ref modifiers);
        }

        private static void ApplyDamageBonus(Player player, ref NPC.HitModifiers modifiers)
        {
            if (player.statLife >= player.statLifeMax2 * HealthThreshold)
                modifiers.FinalDamage *= DamageMultiplier;
        }
    }
}
