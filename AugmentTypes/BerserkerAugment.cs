using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class BerserkerAugment : Augment
    {
        public override string Id => "berserker";
        public override string DisplayName => "Berserker";
        public override string Description =>
            $"While below {AugmentText.HP("50% HP")}, melee hits deal {AugmentText.BonusDamage("+20% bonus damage")}.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Melee;

        private const float TriggerThreshold = 0.5f;
        private const float BonusDamagePercent = 0.20f;

        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (item.DamageType == DamageClass.Melee && player.statLife < player.statLifeMax2 * TriggerThreshold)
                modifiers.FlatBonusDamage += (int)(item.damage * BonusDamagePercent);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.DamageType == DamageClass.Melee && player.statLife < player.statLifeMax2 * TriggerThreshold)
                modifiers.FlatBonusDamage += (int)(proj.damage * BonusDamagePercent);
        }
    }
}
