using Terraria;
using Terraria.ModLoader;

namespace Augments
{
	// Amplifies every active damage-over-time effect on an NPC by 50% once
	// Festering Wounds is owned, regardless of source. Vanilla debuffs (On
	// Fire!, Frostburn, Poisoned, etc.) and our own custom bleed tracker
	// (AugmentBleedNPC) both work by driving npc.lifeRegen negative each
	// tick - reading the CURRENT total negative lifeRegen here, after every
	// other source has already contributed to it, amplifies all of them at
	// once with no need to detect or special-case individual debuffs.
	//
	// Autoload disabled and registered manually in Augments.Load(), strictly
	// after AugmentBleedNPC - this guarantees UpdateLifeRegen runs after
	// vanilla's own debuff calculations (which always finish before any
	// GlobalNPC.UpdateLifeRegen fires) AND after AugmentBleedNPC's bleed
	// contribution for this same tick. tModLoader doesn't guarantee autoload
	// order between two separate GlobalNPCs, so this can't be left implicit.
	[Autoload(false)]
	public class AugmentFesteringWoundsNPC : GlobalNPC
	{
		private const float Amplification = 1.5f;

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (npc.lifeRegen >= 0)
				return;

			// UpdateLifeRegen gets no attacker/player parameter, and this hook runs
			// identically on every client plus the server - checking Main.LocalPlayer
			// would check whichever player happens to be local to whichever machine is
			// running this code (nobody, on a dedicated server; the host, not the actual
			// attacker, in hosted play), amplifying the wrong NPC's DoT or none at all.
			// npc.lastInteraction is the synced, network-consistent field that actually
			// identifies who's fighting this NPC - same field AugmentGlobalNPC.OnKill
			// already uses to attribute a kill to the right player.
			if (npc.lastInteraction < 0 || npc.lastInteraction >= Main.maxPlayers)
				return;

			Player attacker = Main.player[npc.lastInteraction];
			if (!attacker.active || !attacker.GetModPlayer<AugmentPlayer>().HasAugment("festering_wounds"))
				return;

			npc.lifeRegen = (int)(npc.lifeRegen * Amplification);
		}
	}
}
