using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    public class AugmentGlobalNPC : GlobalNPC
    {
        private const float TauntRadius = 800f;

        // Redirect hostile NPCs toward any nearby Support player who owns Taunt.
        // Runs before vanilla AI so the target index is set before movement is calculated.
        // Note: boss AI often re-overrides npc.target mid-AI; Taunt is best-effort for non-bosses.
        public override bool PreAI(NPC npc)
        {
            if (!npc.friendly && !npc.townNPC && !npc.dontTakeDamage)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (!player.active || player.dead) continue;

                    var ap = player.GetModPlayer<AugmentPlayer>();
                    if (!ap.HasAugment("taunt")) continue;
                    if (Vector2.Distance(npc.Center, player.Center) > TauntRadius) continue;

                    npc.target = i;
                    break;
                }
            }

            return true;
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            int essenceType = ModContent.ItemType<AugmentEssenceItem>();

            // Tim — rare skeleton wizard; 100% drop.
            if (npc.type == NPCID.Tim)
                npcLoot.Add(ItemDropRule.Common(essenceType, 1, 1, 1));

            // Nymph — drop belongs on Nymph (the transformed form), not LostGirl.
            if (npc.type == NPCID.Nymph)
                npcLoot.Add(ItemDropRule.Common(essenceType, 1, 1, 1));

            // All mimic variants — 50% drop.
            // BigMimic* are the hardmode biome-key mimics (separate NPCIDs from base Mimic).
            if (npc.type == NPCID.Mimic          ||
                npc.type == NPCID.IceMimic        ||
                npc.type == NPCID.PresentMimic    ||
                npc.type == NPCID.BigMimicCorruption ||
                npc.type == NPCID.BigMimicCrimson    ||
                npc.type == NPCID.BigMimicHallow     ||
                npc.type == NPCID.BigMimicJungle)
            {
                npcLoot.Add(ItemDropRule.Common(essenceType, 2, 1, 1));
            }

            // Dungeon Spirit — hardmode post-Plantera dungeon enemy; 50% drop.
            if (npc.type == NPCID.DungeonSpirit)
                npcLoot.Add(ItemDropRule.Common(essenceType, 2, 1, 1));
        }

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
