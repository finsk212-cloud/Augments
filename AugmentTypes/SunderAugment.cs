using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class SunderAugment : Augment
    {
        public override string Id => "sunder";
        public override string DisplayName => "Sunder";
        public override string Description =>
            $"Melee {AugmentText.Crit("crits")} apply {AugmentText.Ichor("Ichor")} for {AugmentText.Duration("3s")}, reducing enemy defense briefly.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Melee;

        private const int IchorDurationTicks = 180;

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (hit.Crit && item.DamageType == DamageClass.Melee)
                target.AddBuff(BuffID.Ichor, IchorDurationTicks);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (hit.Crit && proj.DamageType == DamageClass.Melee)
                target.AddBuff(BuffID.Ichor, IchorDurationTicks);
        }
    }
}
