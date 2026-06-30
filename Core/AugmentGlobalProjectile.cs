using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Augments
{
	public class AugmentGlobalProjectile : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
				return;

			Player player = Main.player[projectile.owner];

			if (!player.active)
				return;

			foreach (var augment in player.GetModPlayer<AugmentPlayer>().Owned)
				augment.OnProjectileSpawn(player, projectile);
		}
	}
}
