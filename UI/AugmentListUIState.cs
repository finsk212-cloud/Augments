using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace Augments
{
	public class AugmentListUIState : UIState
	{
		private UIPanel backPanel;
		private UIList list;

		public override void OnInitialize()
		{
			backPanel = new UIPanel();
			backPanel.Width.Set(360f, 0f);
			backPanel.Height.Set(420f, 0f);
			backPanel.HAlign = 1f; // right side of screen
			backPanel.VAlign = 0.3f;
			backPanel.Left.Set(-20f, 0f);
			backPanel.BackgroundColor = new Color(33, 43, 79) * 0.9f;

			UIText title = new UIText("Your Augments")
			{
				HAlign = 0.5f
			};
			title.Top.Set(10f, 0f);
			backPanel.Append(title);

			list = new UIList();
			list.Top.Set(45f, 0f);
			list.Width.Set(-30f, 1f);
			list.Height.Set(-55f, 1f);
			list.HAlign = 0f;
			list.Left.Set(10f, 0f);
			list.ListPadding = 6f;
			backPanel.Append(list);

			UIScrollbar scrollbar = new UIScrollbar();
			scrollbar.Top.Set(45f, 0f);
			scrollbar.Height.Set(-55f, 1f);
			scrollbar.HAlign = 1f;
			list.SetScrollbar(scrollbar);
			backPanel.Append(scrollbar);

			// Centered in the 55px gap below the list (list ends at 420-55=365px).
			var supportTag = new SupportClassTagElement();
			supportTag.Width.Set(0f, 1f);
			supportTag.Height.Set(30f, 0f);
			supportTag.Top.Set(377f, 0f);
			backPanel.Append(supportTag);

			Append(backPanel);
		}

		// Rebuilds the list from whatever the local player currently owns.
		// Called explicitly right before the panel is shown - NOT automatically
		// on activation, since this runs too early (before any player exists)
		// if hooked into OnActivate during mod load.
		public void Refresh()
		{
			list.Clear();
			var owned = Main.LocalPlayer.GetModPlayer<AugmentPlayer>().Owned;

			foreach (var augment in owned)
			{
				var entry = new AugmentListEntry(augment);
				entry.Width.Set(0f, 1f);
				entry.Height.Set(50f, 0f);
				list.Add(entry);
			}
		}
	}
}
