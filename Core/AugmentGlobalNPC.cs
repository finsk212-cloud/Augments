using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    public class AugmentGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.friendly || npc.lastInteraction < 0 || npc.lastInteraction >= Main.maxPlayers)
                return;

            Player player = Main.player[npc.lastInteraction];

            if (!player.active)
                return;

            foreach (var augment in player.GetModPlayer<AugmentPlayer>().Owned)
                augment.OnKillNPC(player, npc);
        }
    }
}
