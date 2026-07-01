using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Augments
{
    // Fixed-position tag drawn at the bottom of the augment list panel whenever
    // the local player is in Support stance (2+ Support augments owned).
    // Hover reveals the same stance scaling table shown on the choice card.
    public class SupportClassTagElement : UIElement
    {
        private const string TagText = "SUPPORT CLASS";
        private static readonly Vector2 TagScale = new Vector2(0.65f);
        private static readonly Color TagColorActive   = new Color(100, 220, 140);
        private static readonly Color TagColorInactive = new Color(120, 120, 120);

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var ap = Main.LocalPlayer.GetModPlayer<AugmentPlayer>();
            if (ap.SupportAugmentCount < 1)
                return;

            bool active = ap.SupportAugmentCount >= 2;
            Color color = active ? TagColorActive : TagColorInactive;

            var font = FontAssets.MouseText.Value;
            CalculatedStyle dims = GetDimensions();

            Vector2 tagSize = ChatManager.GetStringSize(font, TagText, TagScale);
            Vector2 tagPos = new Vector2(
                dims.X + dims.Width / 2f - tagSize.X / 2f,
                dims.Y + dims.Height / 2f - tagSize.Y / 2f
            );

            ChatManager.DrawColorCodedStringWithShadow(
                spriteBatch, font, TagText, tagPos,
                color, 0f, Vector2.Zero, TagScale
            );

            if (IsMouseHovering)
                AugmentChoiceCard.DrawSupportTooltip(spriteBatch, font);
        }
    }
}
