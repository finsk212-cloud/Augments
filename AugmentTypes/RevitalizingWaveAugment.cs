using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class RevitalizingWaveAugment : Augment
    {
        private const float AuraRadius = 600f;
        private const int PulseTicks = 1200; // 20 seconds

        public override string Id => "revitalizing_wave";
        public override string DisplayName => "Revitalizing Wave";
        public override string Description =>
            $"Every {AugmentText.Cooldown("20 seconds")}, you start charging a magical cloud. At max 20 stacks heal all nearby teammates for {AugmentText.Healing("25 HP")}.";
        public override AugmentRarity Rarity => AugmentRarity.Epic;
        public override AugmentClass Class => AugmentClass.Support;
        public override bool HasAuraEffect => true;

        private int pulseTimer = PulseTicks;

        public override int CooldownRemaining => pulseTimer;

        public override void OnUpdate(Player player)
        {
            float progress = 1f - (pulseTimer / (float)PulseTicks); // 0 → 1 as it charges

            // Orb center floats above the player's head and rises slightly as it grows.
            Vector2 orbCenter = player.Top + new Vector2(0f, -18f - progress * 14f);

            // Orb shell radius grows from 4px → 20px.
            float orbRadius = 4f + progress * 16f;

            // Spawn more dust as it charges — very dense when fully charged (pulseTimer == 0).
            int dustCount = pulseTimer == 0 ? 2 : (Main.rand.NextBool(3) ? 1 : 0);

            // Spinning ring around the orb.
            for (int d = 0; d < dustCount; d++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float cos = (float)Math.Cos(angle);
                float sin = (float)Math.Sin(angle);
                Vector2 spawnPos = orbCenter + new Vector2(cos, sin) * orbRadius;
                Vector2 tangent = new Vector2(-sin, cos) * (1.5f + progress * 2.5f);

                Dust orb = Dust.NewDustDirect(spawnPos, 0, 0, DustID.GemEmerald);
                orb.noGravity = true;
                orb.velocity = tangent;
                orb.scale = 0.7f + progress * 1.1f;
                orb.alpha = 30 + (int)(progress * 60f);
            }

            // Dense glowing core — packed particles at the center form the visible orb.
            int coreCount = pulseTimer == 0 ? 4 : (int)(1f + progress * 2f);
            for (int d = 0; d < coreCount; d++)
            {
                float coreAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float coreDist = Main.rand.NextFloat() * orbRadius * 0.35f;
                Vector2 corePos = orbCenter + new Vector2((float)Math.Cos(coreAngle), (float)Math.Sin(coreAngle)) * coreDist;

                Dust core = Dust.NewDustDirect(corePos, 0, 0, DustID.GemEmerald);
                core.noGravity = true;
                core.velocity = Vector2.Zero;
                core.scale = 0.6f + progress * 1.0f;
                core.alpha = 0; // fully opaque — bright glowing center
            }

            if (pulseTimer > 0)
            {
                pulseTimer--;
                return;
            }

            // Timer at 0 — hold until at least one teammate is in range.
            bool hasTarget = false;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (!other.active || other.dead || other == player) continue;
                if (Vector2.Distance(player.Center, other.Center) <= AuraRadius)
                {
                    hasTarget = true;
                    break;
                }
            }
            if (!hasTarget) return;

            // Fire — burst the orb outward from above the head.
            pulseTimer = PulseTicks;
            SpawnBurst(player, orbCenter);

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (!other.active || other.dead || other == player) continue;
                if (Vector2.Distance(player.Center, other.Center) > AuraRadius) continue;

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    other.Heal(25);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = ModLoader.GetMod("Augments").GetPacket();
                    packet.Write((byte)AugmentPacketType.RevitalizingWaveHeal);
                    packet.Write((byte)other.whoAmI);
                    packet.Write(25);
                    packet.Send();
                }
            }
        }

        private static void SpawnBurst(Player player, Vector2 orbCenter)
        {
            // Tight ring erupting from the orb position.
            for (int i = 0; i < 36; i++)
            {
                float angle = i / 36f * MathHelper.TwoPi;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle))
                              * Main.rand.NextFloat(4f, 8f);

                Dust d = Dust.NewDustDirect(orbCenter, 0, 0, DustID.GemEmerald);
                d.noGravity = true;
                d.velocity = vel;
                d.scale = 1.5f + Main.rand.NextFloat(0.6f);
            }

            // Trailing motes that arc downward from the orb toward the ground.
            for (int i = 0; i < 20; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle))
                              * Main.rand.NextFloat(3f, 7f);

                Dust d = Dust.NewDustDirect(orbCenter, 0, 0, DustID.GemEmerald);
                d.noGravity = false;
                d.velocity = vel;
                d.scale = 0.9f + Main.rand.NextFloat(0.5f);
            }
        }
    }
}
