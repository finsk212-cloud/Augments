using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class EldritchEyeProjectile : ModProjectile
    {
        private const int FireIntervalTicks = 30;
        private const float AttackRange = 450f;

        public override string Texture => "Terraria/Images/NPC_" + NPCID.MoonLordFreeEye;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = Main.npcFrameCount[NPCID.MoonLordFreeEye];
        }

        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 54;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.alpha = 35;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.localAI[0]++;
            Projectile.rotation = MathF.Sin(Projectile.localAI[0] * 0.05f) * 0.08f;
            Projectile.scale = 0.7f + MathF.Sin(Projectile.localAI[0] * 0.08f) * 0.04f;
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }

            Lighting.AddLight(Projectile.Center, 0.22f, 0.05f, 0.32f);
            SpawnEyeMotes();

            if ((int)Projectile.localAI[0] % FireIntervalTicks != 0 || Projectile.owner != Main.myPlayer)
                return;

            NPC target = FindNearestTarget();
            if (target == null)
                return;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<EldritchLaserProjectile>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner,
                Projectile.identity,
                Projectile.DirectionTo(target.Center).ToRotation(),
                Main.rand.NextBool() ? 1f : -1f
            );
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(195, 80, 255, 220);
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            SpawnBurst(18, 3.5f);
        }

        public override void OnKill(int timeLeft)
        {
            SpawnBurst(24, 5f);
        }

        private void SpawnEyeMotes()
        {
            if ((int)Projectile.localAI[0] % 3 != 0)
                return;

            float angle = Projectile.localAI[0] * 0.11f;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 offset = new Vector2(MathF.Cos(angle) * 34f * side, MathF.Sin(angle) * 18f);
                Vector2 tangent = new Vector2(-MathF.Sin(angle), MathF.Cos(angle)) * 0.65f * side;
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + offset,
                    side == 1 ? DustID.GemAmethyst : DustID.Shadowflame,
                    tangent,
                    130,
                    default,
                    0.75f
                );
                dust.noGravity = true;
            }
        }

        private void SpawnBurst(int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    i % 3 == 0 ? DustID.GemSapphire : DustID.GemAmethyst,
                    angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.45f, speed),
                    110,
                    default,
                    Main.rand.NextFloat(0.7f, 1.15f)
                );
                dust.noGravity = true;
            }
        }

        private NPC FindNearestTarget()
        {
            NPC nearest = null;
            float nearestDistanceSquared = AttackRange * AttackRange;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy(Projectile))
                    continue;

                float distanceSquared = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distanceSquared >= nearestDistanceSquared)
                    continue;

                nearest = npc;
                nearestDistanceSquared = distanceSquared;
            }

            return nearest;
        }
    }
}
