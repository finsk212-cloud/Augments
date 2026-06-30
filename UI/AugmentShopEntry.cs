using System;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Augments
{
	// A single row in the shop's "Buy Back" or "Remove" list. Mirrors
	// AugmentListEntry's hover-tooltip behavior (sets the shared
	// AugmentListEntry.HoveredAugment field so AugmentTooltipDrawer picks it
	// up unclipped), but also hosts a clickable action button on the right.
	public class AugmentShopEntry : UIPanel
	{
		private readonly Augment augment;

		public AugmentShopEntry(Augment augment, string actionLabel, Action<Augment> onAction)
		{
			this.augment = augment;

			BackgroundColor = AugmentListEntry.RarityColor(augment.Rarity) * 0.3f;
			BorderColor = AugmentListEntry.RarityColor(augment.Rarity);
			SetPadding(0f);

			UIText nameText = new UIText(augment.DisplayName, 0.8f)
			{
				HAlign = 0f,
				VAlign = 0.5f
			};
			nameText.Left.Set(10f, 0f);
			Append(nameText);

			var actionButton = new ActionButton(actionLabel);
			actionButton.Width.Set(150f, 0f);
			actionButton.Height.Set(0f, 0.7f);
			actionButton.HAlign = 1f;
			actionButton.VAlign = 0.5f;
			actionButton.Left.Set(-10f, 0f);
			actionButton.Clicked += () => onAction(augment);
			Append(actionButton);
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			BackgroundColor = AugmentListEntry.RarityColor(augment.Rarity) * 0.6f;
			AugmentListEntry.HoveredAugment = augment;
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			base.MouseOut(evt);
			BackgroundColor = AugmentListEntry.RarityColor(augment.Rarity) * 0.3f;
			if (AugmentListEntry.HoveredAugment == augment)
				AugmentListEntry.HoveredAugment = null;
		}

		// Small standalone clickable panel - same manual hover/click approach
		// AugmentChoiceCard uses, rather than vanilla's UITextPanel, to match
		// this mod's existing UI button style.
		private class ActionButton : UIPanel
		{
			public event Action Clicked;

			private static readonly Color IdleColor = new Color(60, 70, 110);
			private static readonly Color HoverColor = new Color(90, 105, 160);

			public ActionButton(string label)
			{
				SetPadding(0f);
				BackgroundColor = IdleColor;
				BorderColor = Color.White * 0.4f;

				UIText labelText = new UIText(label, 0.75f)
				{
					HAlign = 0.5f,
					VAlign = 0.5f
				};
				Append(labelText);
			}

			public override void LeftClick(UIMouseEvent evt)
			{
				base.LeftClick(evt);
				Clicked?.Invoke();
			}

			public override void MouseOver(UIMouseEvent evt)
			{
				base.MouseOver(evt);
				BackgroundColor = HoverColor;
			}

			public override void MouseOut(UIMouseEvent evt)
			{
				base.MouseOut(evt);
				BackgroundColor = IdleColor;
			}
		}
	}
}
