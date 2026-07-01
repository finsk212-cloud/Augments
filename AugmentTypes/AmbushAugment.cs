using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class AmbushAugment : Augment
    {
        public override string Id => "ambush";
        public override string DisplayName => "Ambush";
        public override string Description =>
            $"Stand still for {AugmentText.Duration("2s")} to prime your next melee hit. The primed hit deals " +
            $"{AugmentText.BonusDamage("+25% bonus damage")} based on the weapon's base damage.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Melee;

        private const int StillTicksRequired = 120;
        private const float BonusDamagePercent = 0.25f;

        private int stillTicks;
        private bool ready;

        public override void OnUpdate(Player player)
        {
            bool isStill = player.velocity.LengthSquared() < 0.01f;

            if (!isStill)
            {
                stillTicks = 0;
                return;
            }

            if (ready)
                return;

            stillTicks++;
            if (stillTicks == StillTicksRequired)
            {
                ready = true;
                Main.NewText("Ambush primed!", 200, 200, 255);
            }
        }

        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (ready && item.CountsAsClass(DamageClass.Melee))
                Consume(ref modifiers, item.damage);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (ready && proj.CountsAsClass(DamageClass.Melee))
                Consume(ref modifiers, proj.damage);
        }

        private void Consume(ref NPC.HitModifiers modifiers, int baseDamage)
        {
            modifiers.FlatBonusDamage += (int)(baseDamage * BonusDamagePercent);
            ready = false;
            stillTicks = 0;
        }
    }
}
