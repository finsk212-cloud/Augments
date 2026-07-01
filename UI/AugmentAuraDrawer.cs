using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace Augments
{
	// Draws a faint green ring at the 600px aura radius around every active
	// Support-stance player who owns at least one aura augment. Uses
	// InterfaceScaleType.Game so the spriteBatch matrix is Main.GameViewMatrix
	// (the actual camera transform) - world coordinates are passed directly,
	// no manual screenPosition subtraction needed.
	public static class AugmentAuraDrawer
	{
		private const float AuraRadius = 600f;
		private const int CircleSteps = 64;
		private const float LineThickness = 2f;
		private static readonly Color CircleColor = new Color(100, 220, 140) * 0.22f;

		public static void DrawAuras(SpriteBatch spriteBatch)
		{
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				if (!player.active || player.dead)
					continue;

				var ap = player.GetModPlayer<AugmentPlayer>();
				if (ap.SupportAugmentCount < 2)
					continue;

				if (!ap.HasAugment("warcry") && !ap.HasAugment("ironclad_aura"))
					continue;

				DrawCircle(spriteBatch, player.Center);
			}
		}

		private static void DrawCircle(SpriteBatch spriteBatch, Vector2 worldCenter)
		{
			for (int s = 0; s < CircleSteps; s++)
			{
				float a1 = s / (float)CircleSteps * MathHelper.TwoPi;
				float a2 = (s + 1) / (float)CircleSteps * MathHelper.TwoPi;

				Vector2 p1 = worldCenter + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * AuraRadius;
				Vector2 p2 = worldCenter + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * AuraRadius;

				DrawSegment(spriteBatch, p1, p2);
			}
		}

		private static void DrawSegment(SpriteBatch spriteBatch, Vector2 start, Vector2 end)
		{
			Vector2 delta = end - start;
			float len = delta.Length();
			if (len < 0.5f)
				return;

			float angle = (float)Math.Atan2(delta.Y, delta.X);
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				start,
				null,
				CircleColor,
				angle,
				new Vector2(0f, 0.5f), // pivot at left-center so rotation is correct
				new Vector2(len, LineThickness),
				SpriteEffects.None,
				0f
			);
		}
	}
}
