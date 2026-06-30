using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class VengeanceAugment : Augment
    {
        public override string Id => "vengeance";
        public override string DisplayName => "Vengeance";
        public override string Description =>
            $"When an enemy hits you with a direct attack (not their projectiles), it instantly takes " +
            $"{AugmentText.BonusDamage("8 damage")} back, reduced by its defense like a normal hit.";

        public override AugmentRarity Rarity => AugmentRarity.Rare;
        public override AugmentClass Class => AugmentClass.Universal;

        private const int RetaliationDamage = 8;

        public override void OnHitByNPC(Player player, NPC npc, Player.HurtInfo hurtInfo)
        {
            npc.SimpleStrikeNPC(RetaliationDamage, player.direction);
        }
    }
}
