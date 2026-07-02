using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class MinionMomentumAugment : Augment
    {
        public override string Id => "minion_momentum";
        public override string DisplayName => "Minion Momentum";
        public override string Description =>
            $"Minion hits build a stacking {AugmentText.BonusDamage("+1% bonus damage")} per hit, up to " +
            $"{AugmentText.BonusDamage("+20% bonus damage")} at 20 stacks, as long as the same minion " +
            "type stays active. Switching to a different summon weapon resets the stack.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Summon;

        // Placeholder until we have custom art.
        public override Texture2D Icon => TextureAssets.Item[ItemID.SlimeStaff].Value;

        private const int MaxStacks = 20;
        private const float BonusPerStackPercent = 0.01f;

        public override bool IsCharging => LocalPlayerState.MinionMomentumHitStacks > 0;
        public override int ChargeIndicatorPercent => LocalPlayerState.MinionMomentumHitStacks;

        // Detection only - whip swings (SummonMeleeSpeed via OnHitNPCWithItem)
        // are the player tagging a target, not the minion itself attacking, so
        // they don't establish or change "current minion type". Only an actual
        // minion-owned projectile hit does (the isMinionDamage guard below
        // already excludes whips, which are neither minion nor MinionShot).
        public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
        {
            // proj.minion is only true for minions that damage via their own
            // body's contact (Pygmies, Spider Staff, etc). Minions that instead
            // fire a separate attack projectile (Hornet Staff's stinger, Imp
            // Staff's fireball, etc) are NOT flagged minion=true on that shot -
            // they're tracked via ProjectileID.Sets.MinionShot instead. Missing
            // this is exactly why hits from those minions weren't registering.
            bool isMinionDamage = proj.minion || ProjectileID.Sets.MinionShot[proj.type];

            // CountsAsClass (not ==) so modded minions whose damage class merely
            // derives from Summon still register.
            if (!proj.CountsAsClass(DamageClass.Summon) || !isMinionDamage)
                return;

            var ap = player.GetModPlayer<AugmentPlayer>();
            if (proj.type != ap.MinionMomentumActiveMinionProjType)
            {
                ap.MinionMomentumActiveMinionProjType = proj.type;
                ap.MinionMomentumHitStacks = 0;
            }

            if (ap.MinionMomentumHitStacks < MaxStacks)
                ap.MinionMomentumHitStacks++;
        }

        // Damage application - covers both whip taps and minion hits, since
        // both are Summon-class damage the player should feel benefit from.
        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (item.CountsAsClass(DamageClass.Summon))
                modifiers.FlatBonusDamage += (int)(item.damage * player.GetModPlayer<AugmentPlayer>().MinionMomentumHitStacks * BonusPerStackPercent);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.CountsAsClass(DamageClass.Summon))
                modifiers.FlatBonusDamage += (int)(proj.damage * player.GetModPlayer<AugmentPlayer>().MinionMomentumHitStacks * BonusPerStackPercent);
        }
    }
}
