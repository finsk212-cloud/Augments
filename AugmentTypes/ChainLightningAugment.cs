using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class ChainLightningAugment : Augment
    {
        public override string Id => "chain_lightning";
        public override string DisplayName => "Chain Lightning";
        public override string Description =>
            $"Melee {AugmentText.Crit("crits")} chains damage to nearby enemies, dealing 50% of {AugmentText.Trigger("on hit")} damage to up to 2 targets within range.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Melee;

        private const float ChainRange = 200f;
        private const int MaxChainTargets = 2;
        private const float ChainDamageFraction = 0.5f;

        public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
        {
            if (hit.Crit && item.DamageType == DamageClass.Melee)
                ChainToNearbyTargets(target, hit);
        }

        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            if (hit.Crit && proj.DamageType == DamageClass.Melee)
                ChainToNearbyTargets(target, hit);
        }

        private static void ChainToNearbyTargets(NPC target, NPC.HitInfo hit)
        {
            var nearby = new List<NPC>();
            foreach (NPC npc in Main.npc)
            {
                if (npc == target || !npc.active || npc.friendly || npc.townNPC)
                    continue;

                if (npc.Distance(target.Center) <= ChainRange)
                    nearby.Add(npc);
            }

            nearby.Sort((a, b) => a.Distance(target.Center).CompareTo(b.Distance(target.Center)));

            int chainDamage = (int)(hit.Damage * ChainDamageFraction);
            for (int i = 0; i < nearby.Count && i < MaxChainTargets; i++)
            {
                NPC chainTarget = nearby[i];
                int direction = chainTarget.Center.X >= target.Center.X ? 1 : -1;
                chainTarget.SimpleStrikeNPC(chainDamage, direction, false, 0f, DamageClass.Melee, false, 0f, true);
            }
        }
    }
}
