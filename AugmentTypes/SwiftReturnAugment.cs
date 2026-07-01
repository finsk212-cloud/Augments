using Terraria;

namespace Augments
{
    public class SwiftReturnAugment : Augment
    {
        public override string Id => "swift_return";
        public override string DisplayName => "Swift Return";
        public override string Description =>
            $"On death, your respawn timer is reduced by {AugmentText.Cooldown("40%")}.";
        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Universal;

        public override void OnKill(Player player)
        {
            player.respawnTimer = (int)(player.respawnTimer * 0.60f);
        }
    }
}
