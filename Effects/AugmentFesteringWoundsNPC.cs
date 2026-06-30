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

			if (!Main.LocalPlayer.GetModPlayer<AugmentPlayer>().HasAugment("festering_wounds"))
				return;

			npc.lifeRegen = (int)(npc.lifeRegen * Amplification);
		}
	}
}
