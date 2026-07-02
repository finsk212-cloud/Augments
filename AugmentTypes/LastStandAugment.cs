using Terraria;

namespace Augments
{
    public class LastStandAugment : Augment
    {
        public override string Id => "last_stand";
        public override string DisplayName => "Last Stand";
        public override string Description =>
            $"A killing blow instead leaves you at {AugmentText.HP("1 HP")}, with brief invulnerability. " +
            $"{AugmentText.Cooldown("3 minutes cooldown")}.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int CooldownTicks = 10800;
        private const int InvulnerabilityTicks = 120;

        public override int CooldownRemaining => LocalPlayerState.LastStandCooldown;

        public override void OnUpdate(Player player)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();
            if (ap.LastStandCooldown > 0)
                ap.LastStandCooldown--;
        }

        public override void ModifyHurt(Player player, ref Player.HurtModifiers modifiers)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();
            ap.LastStandArmedThisHit = false;

            if (ap.LastStandCooldown > 0 || player.statLife <= 1)
                return;

            modifiers.SetMaxDamage(player.statLife - 1);
            ap.LastStandArmedThisHit = true;
        }

        public override void OnHurt(Player player, Player.HurtInfo info)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();
            if (!ap.LastStandArmedThisHit)
                return;

            ap.LastStandArmedThisHit = false;

            if (info.Damage != player.statLife - 1)
                return;

            ap.LastStandCooldown = CooldownTicks;
            player.immune = true;
            player.immuneTime = InvulnerabilityTicks;
        }
    }
}
