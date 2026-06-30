using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace Augments
{
    public class PhoenixHeartAugment : Augment
    {
        public override string Id => "phoenix_heart";
        public override string DisplayName => "Phoenix Heart";
        public override string Description =>
            $"A killing blow instead restores you to {AugmentText.Healing("max HP")}, with " +
            $"{AugmentText.Duration("3s")} of invulnerability, followed by a {AugmentText.Cooldown("12hr cooldown")}.";

        public override AugmentRarity Rarity => AugmentRarity.Legendary;
        public override AugmentClass Class => AugmentClass.Universal;

        // Placeholder until we have custom art - Life Fruit's glowing,
        // vitality-themed look fits a "full revive" effect.
        public override Texture2D Icon => TextureAssets.Item[ItemID.LifeFruit].Value;

        private const int InvulnerabilityTicks = 180;

        // 12 in-game hours * 3600 ticks/hour.
        private const int CooldownTicks = 43200;

        private bool armedThisHit;

        private int cooldownRemaining;
        private int invulnTicksRemaining;

        public override bool CooldownDisplayInHours => true;

        public override int CooldownRemaining => cooldownRemaining;

        public override void OnUpdate(Player player)
        {
            if (cooldownRemaining > 0)
                cooldownRemaining--;

            // Vanilla's own "just took damage" invulnerability assignment
            // overwrites player.immuneTime with its own short window right
            // after our trigger sets it - re-force our value every tick
            // instead of setting it once and hoping it survives.
            if (invulnTicksRemaining > 0)
            {
                player.immune = true;
                player.immuneTime = invulnTicksRemaining;
                invulnTicksRemaining--;

                // Cooldown starts once invulnerability actually runs out, not
                // the instant the trigger fires, so the full 12 hours is spent
                // on cooldown rather than overlapping with the invuln window.
                if (invulnTicksRemaining == 0)
                    cooldownRemaining = CooldownTicks;
            }
        }

        public override void ModifyHurt(Player player, ref Player.HurtModifiers modifiers)
        {
            armedThisHit = false;

            if (cooldownRemaining > 0 || player.statLife <= 1)
                return;

            modifiers.SetMaxDamage(player.statLife - 1);
            armedThisHit = true;
        }

        public override void OnHurt(Player player, Player.HurtInfo info)
        {
            if (!armedThisHit)
                return;

            armedThisHit = false;

            if (info.Damage != player.statLife - 1)
                return;

            // OnHurt fires before vanilla subtracts info.Damage from statLife,
            // so add it back now - the post-subtraction result lands exactly
            // on statLifeMax2 instead of getting overwritten by the killing blow.
            player.statLife = player.statLifeMax2 + info.Damage;
            player.immune = true;
            invulnTicksRemaining = InvulnerabilityTicks;
        }
    }
}
