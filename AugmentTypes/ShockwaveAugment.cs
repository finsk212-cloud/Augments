using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class ShockwaveAugment : Augment
    {
        public override string Id => "shockwave";
        public override string DisplayName => "Shockwave";
        public override string Description =>
            $"Melee {AugmentText.Crit("crits")} push all nearby enemies away with knockback. No damage or debuffs, just movement.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Melee;

        private const float ShockwaveRange = 150f;
        private const float KnockbackStrength = 6f;

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (hit.Crit && item.DamageType == DamageClass.Melee)
                PushNearbyTargets(player, target);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (hit.Crit && proj.DamageType == DamageClass.Melee)
                PushNearbyTargets(player, target);
        }

        private static void PushNearbyTargets(Player player, NPC target)
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.townNPC)
                    continue;

                if (npc.Distance(target.Center) > ShockwaveRange)
                    continue;

                Vector2 direction = npc.Center - player.Center;
                if (direction == Vector2.Zero)
                    continue;

                direction.Normalize();
                npc.velocity = direction * KnockbackStrength * npc.knockBackResist;
                npc.netUpdate = true;
            }
        }
    }
}
