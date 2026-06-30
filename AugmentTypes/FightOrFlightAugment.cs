using Terraria;

namespace Augments
{
    public class FightOrFlightAugment : Augment
    {
        public override string Id => "fight_or_flight";
        public override string DisplayName => "Fight or Flight";
        public override string Description =>
            $"While below {AugmentText.HP("50% HP")}, gain +10% {AugmentText.MovementSpeed("movement speed")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        private const float TriggerThreshold = 0.5f;
        private const float MovementSpeedBonus = 0.10f;

        public override void PostUpdateRunSpeeds(Player player)
        {
            if (player.statLife <= 0 || player.statLife >= player.statLifeMax2 * TriggerThreshold)
                return;

            player.maxRunSpeed *= 1f + MovementSpeedBonus;
            player.accRunSpeed *= 1f + MovementSpeedBonus;
            player.runAcceleration *= 1f + MovementSpeedBonus;
        }
    }
}
