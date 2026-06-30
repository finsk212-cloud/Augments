using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Augments
{
	// One clickable "card" in the choice panel, representing a single augment option.
	// Text is drawn directly via ChatManager (same approach as AugmentTooltipDrawer)
	// instead of UIText, since UIText's built-in word-wrap doesn't treat
	// [c/RRGGBB:...] tags as atomic and can split them mid-tag, leaving raw
	// tag text on screen.
	public class AugmentChoiceCard : UIPanel
	{
		public Augment Augment { get; }
		public event Action<Augment> OnAugmentChosen;

		public const float CardPadding = 11f;
		private const float NameDescSpacing = 8f;
		private const float LineSpacing = -1f;
		public const float MinCardHeight = 370f;

		private static readonly Vector2 NameScale = new Vector2(0.9f);
		private static readonly Vector2 DescScale = new Vector2(0.8f);

		// Small warning tag shown above the name on any Keystone card, so the
		// "this is a permanent, mutually-exclusive choice" implication is
		// visible before the player ever clicks - the actual confirmation
		// gate lives in AugmentChoiceUIState, this is just the heads-up.
		private const string KeystoneTagText = "KEYSTONE - PERMANENT CHOICE";
		private const float KeystoneTagSpacing = 6f;
		private static readonly Vector2 KeystoneTagScale = new Vector2(0.6f);
		private static readonly Color KeystoneTagColor = new Color(220, 60, 60);

		// Higher rarity = faster/brighter border pulse. Index matches AugmentRarity.
		private static readonly float[] PulseSpeed = { 0f, 1.6f, 2.4f, 3.4f };
		private static readonly float[] PulseStrength = { 0f, 0.18f, 0.32f, 0.5f };

		private readonly List<string> nameLines;
		private readonly List<string> descLines;
		private readonly bool isKeystone;
		private readonly Color baseBorderColor;
		private readonly float pulseSpeed;
		private readonly float pulseStrength;
		private float pulseTimer;

		public AugmentChoiceCard(Augment augment, float width)
		{
			Augment = augment;
			isKeystone = augment.KeystoneFamily != null;

			baseBorderColor = RarityColor(augment.Rarity);
			BackgroundColor = baseBorderColor * 0.35f;
			BorderColor = baseBorderColor;

			int rarityIndex = (int)augment.Rarity;
			pulseSpeed = PulseSpeed[rarityIndex];
			pulseStrength = PulseStrength[rarityIndex];

			SetPadding(CardPadding);
			Width.Set(width, 0f);
			Height.Set(MinCardHeight, 0f);

			var font = FontAssets.MouseText.Value;
			// Derived from the card's own Width (just set above), not a
			// hardcoded/shared constant - this is what the wrap below and
			// the actual on-screen inner area both have to agree on.
			float contentWidth = Width.Pixels - PaddingLeft - PaddingRight;

			nameLines = AugmentColorText.Wrap(font, augment.DisplayName, contentWidth, NameScale);
			descLines = AugmentColorText.Wrap(font, augment.Description, contentWidth, DescScale);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			pulseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			// 0..1 sine pulse; rarities with no pulseStrength stay flat at 0.
			float pulse = pulseStrength > 0f ? (float)Math.Sin(pulseTimer * pulseSpeed) * 0.5f + 0.5f : 0f;

			if (pulse > 0f)
			{
				CalculatedStyle dims = GetDimensions();
				float glowSize = 4f + pulse * pulseStrength * 18f;
				Rectangle glowRect = new Rectangle(
					(int)(dims.X - glowSize),
					(int)(dims.Y - glowSize),
					(int)(dims.Width + glowSize * 2f),
					(int)(dims.Height + glowSize * 2f));

				spriteBatch.Draw(TextureAssets.MagicPixel.Value, glowRect, baseBorderColor * (pulse * pulseStrength * 0.5f));
			}

			BorderColor = Color.Lerp(baseBorderColor, Color.White, pulse * pulseStrength);

			base.DrawSelf(spriteBatch);

			var font = FontAssets.MouseText.Value;
			CalculatedStyle inner = GetInnerDimensions();
			float centerX = inner.X + inner.Width / 2f;
			float y = inner.Y;

			if (isKeystone)
			{
				Vector2 tagSize = ChatManager.GetStringSize(font, KeystoneTagText, KeystoneTagScale);

				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					font,
					KeystoneTagText,
					new Vector2(centerX - tagSize.X / 2f, y),
					KeystoneTagColor,
					0f,
					Vector2.Zero,
					KeystoneTagScale
				);

				y += tagSize.Y + KeystoneTagSpacing;
			}

			y = DrawCenteredLines(spriteBatch, font, nameLines, NameScale, centerX, y);
			y += NameDescSpacing;
			DrawLeftAlignedLines(spriteBatch, font, descLines, DescScale, inner.X, y);
		}

		private static float DrawCenteredLines(SpriteBatch spriteBatch, DynamicSpriteFont font, List<string> lines, Vector2 scale, float centerX, float y)
		{
			foreach (var line in lines)
			{
				Vector2 size = ChatManager.GetStringSize(font, line, scale);

				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					font,
					line,
					new Vector2(centerX - size.X / 2f, y),
					Color.White,
					0f,
					Vector2.Zero,
					scale
				);

				y += size.Y + LineSpacing;
			}

			return y;
		}

		private static float DrawLeftAlignedLines(SpriteBatch spriteBatch, DynamicSpriteFont font, List<string> lines, Vector2 scale, float x, float y)
		{
			foreach (var line in lines)
			{
				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					font,
					line,
					new Vector2(x, y),
					Color.White,
					0f,
					Vector2.Zero,
					scale
				);

				y += ChatManager.GetStringSize(font, line, scale).Y + LineSpacing;
			}

			return y;
		}

		public override void LeftClick(UIMouseEvent evt)
		{
			base.LeftClick(evt);
			SoundEngine.PlaySound(SoundID.MenuTick);
			OnAugmentChosen?.Invoke(Augment);
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			BackgroundColor = RarityColor(Augment.Rarity) * 0.55f;
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			base.MouseOut(evt);
			BackgroundColor = RarityColor(Augment.Rarity) * 0.35f;
		}

		private static Color RarityColor(AugmentRarity rarity)
		{
			switch (rarity)
			{
				case AugmentRarity.Rare:
					return Color.SkyBlue;
				case AugmentRarity.Epic:
					return Color.MediumPurple;
				case AugmentRarity.Legendary:
					return Color.Orange;
				default:
					return Color.White;
			}
		}
	}
}
