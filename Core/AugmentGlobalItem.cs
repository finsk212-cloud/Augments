using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class AugmentGlobalItem : GlobalItem
    {
        public override void OnConsumeItem(Item item, Player player)
        {
            foreach (var augment in player.GetModPlayer<AugmentPlayer>().Owned)
                augment.OnConsumeItem(player, item);
        }
    }
}
