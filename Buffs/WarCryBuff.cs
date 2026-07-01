using Terraria;
using Terraria.ModLoader;

namespace Augments
{
	// Applied each tick by the pull-based Warcry aura in AugmentPlayer.PostUpdate.
	// Duration is intentionally short (10 ticks) so it falls off within a third of
	// a second once the source Support player leaves range, with no manual removal needed.
	// Texture: placeholder - flag for art pass.
	public class WarCryBuff : ModBuff
	{
		public override string Texture => "Terraria/Images/Buff_2";

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetDamage(DamageClass.Generic) += 0.10f;
		}
	}
}
