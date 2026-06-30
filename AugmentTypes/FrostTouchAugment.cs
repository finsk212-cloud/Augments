using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class FrostTouchAugment : Augment
    {
        public override string Id => "frost_touch";
        public override string DisplayName => "Frost Touch";
        public override string Description =>
            $"Magic {AugmentText.Crit("crits")} apply {AugmentText.Frostburn("Frostburn")} for {AugmentText.Duration("4s")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Magic;

        private const int FrostburnDurationTicks = 240;

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (hit.Crit && item.DamageType == DamageClass.Magic)
                target.AddBuff(BuffID.Frostburn, FrostburnDurationTicks);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (hit.Crit && proj.DamageType == DamageClass.Magic)
                target.AddBuff(BuffID.Frostburn, FrostburnDurationTicks);
        }
    }
}
