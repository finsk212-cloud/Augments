using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Augments
{
	// A single row in the "Your Augments" list. Shows the name always;
	// hovering sets HoveredAugment, which a separate top-level layer
	// (AugmentTooltipDrawer) uses to draw the tooltip box unclipped.
	public class AugmentListEntry : UIPanel
	{
		private readonly Augment augment;

		// Shared across all entries. Tracking it here (rather than drawing
		// the tooltip directly in this panel) avoids the tooltip being
		// clipped to this panel's own bounds.
		public static Augment HoveredAugment;

		public AugmentListEntry(Augment augment)
		{
			this.augment = augment;

			BackgroundColor = RarityColor(augment.Rarity) * 0.3f;
			BorderColor = RarityColor(augment.Rarity);

			UIText nameText = new UIText(augment.DisplayName, 0.85f)
			{
				HAlign = 0f,
				VAlign = 0.5f
			};
			nameText.Left.Set(10f, 0f);
			Append(nameText);
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			BackgroundColor = RarityColor(augment.Rarity) * 0.6f;
			HoveredAugment = augment;
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			base.MouseOut(evt);
			BackgroundColor = RarityColor(augment.Rarity) * 0.3f;
			if (HoveredAugment == augment)
				HoveredAugment = null;
		}

		public static Color RarityColor(AugmentRarity rarity)
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
