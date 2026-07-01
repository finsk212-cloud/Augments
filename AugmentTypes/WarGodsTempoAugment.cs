using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class WarGodsTempoAugment : Augment
    {
        public override string Id => "war_gods_tempo";
        public override string DisplayName => "War God's Tempo";
        public override string Description =>
            "Melee hits build Tempo. At 8 Tempo, your next melee hit releases a huge shockwave.";

        public override AugmentRarity Rarity => AugmentRarity.Legendary;
        public override AugmentClass Class => AugmentClass.Melee;

        private const int MaxTempo = 8;
        private const int ResetDelayTicks = 300;
        private const int ShockwaveDamage = 90;

        private int tempo;
        private int tempoResetTimer;

        public override void OnHitNPCWithItem(
            Player player,
            Item item,
            NPC target,
            NPC.HitInfo hit,
            AugmentHitSource source,
            float effectiveness)
        {
            if (source == AugmentHitSource.NormalAttack && item.CountsAsClass(DamageClass.Melee))
                RegisterMeleeHit(player, target);
        }

        public override void OnHitNPCWithProj(
            Player player,
            Projectile proj,
            NPC target,
            NPC.HitInfo hit,
            AugmentHitSource source,
            float effectiveness)
        {
            if (source == AugmentHitSource.NormalAttack && proj.CountsAsClass(DamageClass.Melee))
                RegisterMeleeHit(player, target);
        }

        public override void OnUpdate(Player player)
        {
            if (tempoResetTimer <= 0)
                return;

            tempoResetTimer--;
            if (tempoResetTimer == 0)
                tempo = 0;
        }

        private void RegisterMeleeHit(Player player, NPC target)
        {
            tempoResetTimer = ResetDelayTicks;

            if (tempo >= MaxTempo)
            {
                tempo = 0;
                Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<WarGodShockwaveProjectile>(),
                    ShockwaveDamage,
                    0f,
                    player.whoAmI
                );
                return;
            }

            tempo++;
        }
    }
}
