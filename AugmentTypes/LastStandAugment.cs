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

        private int cooldownRemaining;
        private bool armedThisHit;

        public override int CooldownRemaining => cooldownRemaining;

        public override void OnUpdate(Player player)
        {
            if (cooldownRemaining > 0)
                cooldownRemaining--;
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

            cooldownRemaining = CooldownTicks;
            player.immune = true;
            player.immuneTime = InvulnerabilityTicks;
        }
    }
}
