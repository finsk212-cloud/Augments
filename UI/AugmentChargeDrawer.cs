using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Augments
{
    // Sibling to AugmentCooldownDrawer - same small semi-transparent box +
    // hover tooltip look, but for augments that are actively building toward
    // an effect (counts UP as a percentage, not DOWN to zero). Drawn in its
    // own row just below the cooldown row so the two never overlap.
    public static class AugmentChargeDrawer
    {
        private const float IconSize = 36f;
        private const float Gap = 4f;
        private const float Scale = 0.7f;
        private const float TopOffset = 80f;

        private const float VerticalCenterNudge = 0.15f;
        private const float IconGraphicMaxSize = 28f;

        private readonly struct ChargeIcon
        {
            public readonly string Text;
            public readonly Color Color;
            public readonly string HoverText;
            public readonly Texture2D Icon;

            public ChargeIcon(string text, Color color, string hoverText, Texture2D icon)
            {
                Text = text;
                Color = color;
                HoverText = hoverText;
                Icon = icon;
            }
        }

        public static void DrawCharges(SpriteBatch spriteBatch)
        {
            var augmentPlayer = Main.LocalPlayer.GetModPlayer<AugmentPlayer>();

            var icons = new List<ChargeIcon>();
            foreach (var augment in augmentPlayer.Owned)
            {
                if (!augment.IsCharging)
                    continue;

                icons.Add(new ChargeIcon(
                    $"{augment.ChargeIndicatorPercent}%",
                    AugmentTextColors.Crit,
                    $"{augment.DisplayName} is charging",
                    augment.Icon));
            }

            if (icons.Count == 0)
                return;

            var font = FontAssets.MouseText.Value;
            var scale = new Vector2(Scale);

            float totalWidth = icons.Count * IconSize + Gap * (icons.Count - 1);
            float x = Main.screenWidth / 2f - totalWidth / 2f;

            string hoveredText = null;
            Point mouse = new Point(Main.mouseX, Main.mouseY);

            for (int i = 0; i < icons.Count; i++)
            {
                var boxRect = new Rectangle((int)x, (int)TopOffset, (int)IconSize, (int)IconSize);

                DrawIcon(spriteBatch, font, scale, icons[i].Text, icons[i].Color, icons[i].Icon, boxRect);

                if (boxRect.Contains(mouse))
                    hoveredText = icons[i].HoverText;

                x += IconSize + Gap;
            }

            if (hoveredText != null)
                DrawHoverTooltip(spriteBatch, font, hoveredText);
        }

        private static void DrawIcon(SpriteBatch spriteBatch, DynamicSpriteFont font, Vector2 scale, string text, Color textColor, Texture2D icon, Rectangle boxRect)
        {
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, boxRect, new Color(20, 25, 50) * 0.4f);

            const int border = 1;
            Color borderColor = Color.White * 0.3f;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.X, boxRect.Y, boxRect.Width, border), borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.X, boxRect.Bottom - border, boxRect.Width, border), borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.X, boxRect.Y, border, boxRect.Height), borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.Right - border, boxRect.Y, border, boxRect.Height), borderColor);

            if (icon != null)
            {
                float iconScale = IconGraphicMaxSize / System.Math.Max(icon.Width, icon.Height);
                Vector2 iconSize = new Vector2(icon.Width, icon.Height) * iconScale;
                Vector2 iconPos = new Vector2(
                    boxRect.X + (boxRect.Width - iconSize.X) / 2f,
                    boxRect.Y + (boxRect.Height - iconSize.Y) / 2f
                );

                spriteBatch.Draw(icon, iconPos, null, Color.White, 0f, Vector2.Zero, iconScale, SpriteEffects.None, 0f);
            }

            Vector2 textSize = ChatManager.GetStringSize(font, text, scale);
            Vector2 textPos = new Vector2(
                boxRect.X + (boxRect.Width - textSize.X) / 2f,
                boxRect.Y + (boxRect.Height - textSize.Y) / 2f + textSize.Y * VerticalCenterNudge
            );

            ChatManager.DrawColorCodedStringWithShadow(
                spriteBatch, font, text, textPos,
                textColor, 0f, Vector2.Zero, scale
            );
        }

        private static void DrawHoverTooltip(SpriteBatch spriteBatch, DynamicSpriteFont font, string text)
        {
            const float padding = 10f;

            Vector2 textSize = ChatManager.GetStringSize(font, text, Vector2.One);
            float boxWidth = textSize.X + padding * 2f;
            float boxHeight = textSize.Y + padding * 2f;

            Vector2 boxPos = new Vector2(
                Main.MouseScreen.X - 24f - boxWidth,
                Main.MouseScreen.Y + 24f
            );

            var boxRect = new Rectangle((int)boxPos.X, (int)boxPos.Y, (int)boxWidth, (int)boxHeight);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, boxRect, new Color(20, 25, 50) * 0.95f);

            const int border = 2;
            Color borderColor = Color.White * 0.5f;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.X, boxRect.Y, boxRect.Width, border), borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.X, boxRect.Bottom - border, boxRect.Width, border), borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.X, boxRect.Y, border, boxRect.Height), borderColor);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.Right - border, boxRect.Y, border, boxRect.Height), borderColor);

            ChatManager.DrawColorCodedStringWithShadow(
                spriteBatch, font, text,
                new Vector2(boxRect.X + padding, boxRect.Y + padding),
                Color.White, 0f, Vector2.Zero, Vector2.One
            );
        }
    }
}
