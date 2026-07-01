using Terraria;

namespace Augments
{
    public class CriticalSurgeAugment : Augment
    {
        public override string Id => "critical_surge";
        public override string DisplayName => "Critical Surge";
        public override string Description =>
            $"{AugmentText.Crit("Crits")} have a 30% chance to deal a bonus {AugmentText.Trigger("on-hit")} strike for " +
            $"{AugmentText.SpecialDamage("25%")} of the weapon's base damage.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Universal;

        private const float ProcChance = 0.3f;
        private const float BonusDamagePercent = 0.25f;

        // Not restricted by DamageClass. Built manually (instead of plain
        // SimpleStrikeNPC) so the combat text can be forced to the same blue
        // used for Iron Rhythm/Overcharge Round's popups - HideCombatText
        // suppresses the auto popup and CombatText.NewText spawns our own.
        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (hit.Crit && Main.rand.NextFloat() < ProcChance)
                Strike(player, target, ScaleHitEffect((int)(item.damage * BonusDamagePercent)));
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (hit.Crit && Main.rand.NextFloat() < ProcChance)
                Strike(player, target, ScaleHitEffect((int)(proj.damage * BonusDamagePercent)));
        }

        private static void Strike(Player player, NPC target, int damage)
        {
            var hit = new NPC.HitInfo
            {
                Damage = damage,
                SourceDamage = damage,
                HitDirection = player.direction,
                HideCombatText = true
            };

            target.StrikeNPC(hit);
            CombatText.NewText(target.Hitbox, AugmentTextColors.SpecialDamage, damage);
        }
    }
}
