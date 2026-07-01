using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class RiposteAugment : Augment
    {
        public override string Id => "riposte";
        public override string DisplayName => "Riposte";
        public override string Description =>
            $"After an enemy hits you, you have {AugmentText.Duration("2s")} to land a melee hit. That hit deals " +
            $"{AugmentText.BonusDamage("+20% bonus damage")} based on the weapon's base damage.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Melee;

        private const int WindowTicks = 120;
        private const float BonusDamagePercent = 0.20f;

        private int windowRemaining;

        public override void OnHurt(Player player, Player.HurtInfo info)
        {
            windowRemaining = WindowTicks;
        }

        public override void OnUpdate(Player player)
        {
            if (windowRemaining > 0)
                windowRemaining--;
        }

        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (windowRemaining > 0 && item.CountsAsClass(DamageClass.Melee))
                Consume(ref modifiers, item.damage);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (windowRemaining > 0 && proj.CountsAsClass(DamageClass.Melee))
                Consume(ref modifiers, proj.damage);
        }

        private void Consume(ref NPC.HitModifiers modifiers, int baseDamage)
        {
            modifiers.FlatBonusDamage += (int)(baseDamage * BonusDamagePercent);
            windowRemaining = 0;
        }
    }
}
