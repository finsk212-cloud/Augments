using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class RicochetEngineProjectile : ModProjectile
    {
        private const int MaxHits = 3;
        private const float SearchRadius = 500f;
        private const float MoveSpeed = 14f;
        private const float HomingStrength = 0.18f;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            AugmentProjectileTag tag = Projectile.GetGlobalProjectile<AugmentProjectileTag>();
            tag.IsAugmentProcDamage = true;
            tag.CanTriggerOnHitAugments = true;
            tag.OnHitEffectiveness = 0.5f;
            tag.PreventRicochetEngineCopy = true;

            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = MaxHits;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120;
            Projectile.alpha = 35;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(190, 95, 255, 220);
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                int originalTarget = (int)Projectile.ai[0];
                if (originalTarget >= 0 && originalTarget < Main.maxNPCs)
                    Projectile.localNPCImmunity[originalTarget] = -1;

                SetTarget(FindNearestTarget());
            }

            NPC target = GetCurrentTarget();
            if (target == null)
            {
                SetTarget(FindNearestTarget());
                target = GetCurrentTarget();
            }

            if (target == null)
            {
                Projectile.Kill();
                return;
            }

            Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * MoveSpeed;
            if (Projectile.velocity.LengthSquared() < 0.01f)
                Projectile.velocity = desiredVelocity;
            else
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, HomingStrength);

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.35f, 0.08f, 0.5f);

            Dust dust = Dust.NewDustPerfect(
                Projectile.Center - Projectile.velocity * 0.35f,
                Main.rand.NextBool(3) ? DustID.GemAmethyst : DustID.Shadowflame,
                -Projectile.velocity * 0.08f,
                80,
                default,
                Main.rand.NextFloat(0.65f, 1f)
            );
            dust.noGravity = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.localNPCImmunity[target.whoAmI] = -1;
            Projectile.ai[1]++;
            if (Projectile.ai[1] >= MaxHits)
                return;

            NPC nextTarget = FindNearestTarget();
            if (nextTarget == null)
            {
                Projectile.Kill();
                return;
            }

            SetTarget(nextTarget);
            Projectile.velocity = Projectile.DirectionTo(nextTarget.Center) * MoveSpeed;
            Projectile.netUpdate = true;
        }

        public override bool? CanHitNPC(NPC target)
        {
            int originalTarget = (int)Projectile.ai[0];
            if (target.whoAmI == originalTarget || Projectile.localNPCImmunity[target.whoAmI] != 0)
                return false;

            return null;
        }

        private NPC FindNearestTarget()
        {
            NPC nearest = null;
            float nearestDistanceSquared = SearchRadius * SearchRadius;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy(Projectile) || Projectile.localNPCImmunity[npc.whoAmI] != 0)
                    continue;

                if (npc.whoAmI == (int)Projectile.ai[0])
                    continue;

                float distanceSquared = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distanceSquared >= nearestDistanceSquared)
                    continue;

                nearest = npc;
                nearestDistanceSquared = distanceSquared;
            }

            return nearest;
        }

        private NPC GetCurrentTarget()
        {
            int targetIndex = (int)Projectile.ai[2] - 1;
            if (targetIndex < 0 || targetIndex >= Main.maxNPCs)
                return null;

            NPC target = Main.npc[targetIndex];
            return target.CanBeChasedBy(Projectile) && Projectile.localNPCImmunity[targetIndex] == 0 ? target : null;
        }

        private void SetTarget(NPC target)
        {
            Projectile.ai[2] = target == null ? 0f : target.whoAmI + 1f;
        }
    }
}
