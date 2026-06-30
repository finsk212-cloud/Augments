using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace Augments
{
	// The vendor's shop panel: two side-by-side lists, "Buy" (ever-owned
	// augments not currently held, re-acquirable for Essence) and "Remove"
	// (currently-held augments, free or Essence-refunding depending on
	// rarity). See GetPricing for the actual rarity-based price table.
	public class AugmentShopUIState : UIState
	{
		private UIPanel backPanel;
		private UIText essenceText;
		private UIList buyBackList;
		private UIList removeList;

		private const float PanelWidth = 760f;
		private const float PanelHeight = 440f;
		private const float ListsTop = 90f;

		public override void OnInitialize()
		{
			backPanel = new UIPanel();
			backPanel.Width.Set(PanelWidth, 0f);
			backPanel.Height.Set(PanelHeight, 0f);
			backPanel.HAlign = 0.5f;
			backPanel.VAlign = 0.5f;
			backPanel.BackgroundColor = new Color(33, 43, 79) * 0.9f;

			UIText title = new UIText("Vendor Wares")
			{
				HAlign = 0.5f
			};
			title.Top.Set(10f, 0f);
			backPanel.Append(title);

			var closeButton = new CloseButton();
			closeButton.Width.Set(24f, 0f);
			closeButton.Height.Set(24f, 0f);
			closeButton.HAlign = 1f;
			closeButton.Top.Set(8f, 0f);
			closeButton.Left.Set(-8f, 0f);
			closeButton.Clicked += () => ModContent.GetInstance<AugmentUISystem>().HideShop();
			backPanel.Append(closeButton);

			essenceText = new UIText("");
			essenceText.HAlign = 0.5f;
			essenceText.Top.Set(34f, 0f);
			backPanel.Append(essenceText);

			UIText buyBackHeader = new UIText("Buy")
			{
				HAlign = 0f
			};
			buyBackHeader.Left.Set(10f, 0f);
			buyBackHeader.Top.Set(62f, 0f);
			backPanel.Append(buyBackHeader);

			UIText removeHeader = new UIText("Remove")
			{
				HAlign = 0f
			};
			removeHeader.Left.Set(15f, 0.5f);
			removeHeader.Top.Set(62f, 0f);
			backPanel.Append(removeHeader);

			buyBackList = new UIList();
			buyBackList.Top.Set(ListsTop, 0f);
			buyBackList.Left.Set(10f, 0f);
			buyBackList.Width.Set(-45f, 0.5f);
			buyBackList.Height.Set(-(ListsTop + 10f), 1f);
			buyBackList.ListPadding = 6f;
			backPanel.Append(buyBackList);

			UIScrollbar buyBackScrollbar = new UIScrollbar();
			buyBackScrollbar.Top.Set(ListsTop, 0f);
			buyBackScrollbar.Height.Set(-(ListsTop + 10f), 1f);
			buyBackScrollbar.Left.Set(-25f, 0.5f);
			buyBackList.SetScrollbar(buyBackScrollbar);
			backPanel.Append(buyBackScrollbar);

			removeList = new UIList();
			removeList.Top.Set(ListsTop, 0f);
			removeList.Left.Set(15f, 0.5f);
			removeList.Width.Set(-45f, 0.5f);
			removeList.Height.Set(-(ListsTop + 10f), 1f);
			removeList.ListPadding = 6f;
			backPanel.Append(removeList);

			UIScrollbar removeScrollbar = new UIScrollbar();
			removeScrollbar.Top.Set(ListsTop, 0f);
			removeScrollbar.Height.Set(-(ListsTop + 10f), 1f);
			removeScrollbar.Left.Set(-10f, 1f);
			removeList.SetScrollbar(removeScrollbar);
			backPanel.Append(removeScrollbar);

			Append(backPanel);
		}

		// Rebuilds both lists and the Essence balance readout from the local
		// player's current state. Call explicitly right before showing the
		// panel, same as AugmentListUIState.Refresh.
		public void Refresh()
		{
			buyBackList.Clear();
			removeList.Clear();

			var player = Main.LocalPlayer;
			var augmentPlayer = player.GetModPlayer<AugmentPlayer>();

			foreach (var id in augmentPlayer.EverOwnedIds)
			{
				if (augmentPlayer.HasAugment(id))
					continue;

				var augment = AugmentDatabase.GetById(id);
				if (augment == null)
					continue;

				int buyBackCost = GetPricing(augment.Rarity).buyBackCost;
				var entry = new AugmentShopEntry(augment, $"Buy ({buyBackCost} Essence)", BuyBack);
				entry.Width.Set(0f, 1f);
				entry.Height.Set(50f, 0f);
				buyBackList.Add(entry);
			}

			foreach (var augment in augmentPlayer.Owned)
			{
				int removeRefund = GetPricing(augment.Rarity).removeRefund;
				string label = removeRefund > 0 ? $"Remove (+{removeRefund} Essence)" : "Remove (Free)";
				var entry = new AugmentShopEntry(augment, label, RemoveOwned);
				entry.Width.Set(0f, 1f);
				entry.Height.Set(50f, 0f);
				removeList.Add(entry);
			}

			RefreshEssenceText();
		}

		private void RefreshEssenceText()
		{
			int count = Main.LocalPlayer.CountItem(ModContent.ItemType<AugmentEssenceItem>());
			essenceText.SetText($"Augment Essence: {count}");
		}

		// Remove is free for Common/Rare and refunds Essence for Epic/Legendary;
		// Buy Back always costs Essence, priced strictly higher than any Remove
		// refund for that same rarity - so selling then immediately buying back
		// always costs the player net Essence, never breaks even or profits.
		private static (int removeRefund, int buyBackCost) GetPricing(AugmentRarity rarity)
		{
			switch (rarity)
			{
				case AugmentRarity.Epic:
					return (1, 2);
				case AugmentRarity.Legendary:
					return (2, 4);
				default: // Common, Rare
					return (0, 1);
			}
		}

		private void BuyBack(Augment augment)
		{
			var player = Main.LocalPlayer;
			int essenceType = ModContent.ItemType<AugmentEssenceItem>();
			int cost = GetPricing(augment.Rarity).buyBackCost;

			if (player.CountItem(essenceType, cost) < cost)
			{
				Main.NewText("Not enough Augment Essence.", 255, 80, 80);
				return;
			}

			for (int i = 0; i < cost; i++)
				player.ConsumeItem(essenceType);

			player.GetModPlayer<AugmentPlayer>().GrantAugment(augment);
			Refresh();
		}

		private void RemoveOwned(Augment augment)
		{
			var player = Main.LocalPlayer;
			int refund = GetPricing(augment.Rarity).removeRefund;

			player.GetModPlayer<AugmentPlayer>().RemoveAugment(augment);

			if (refund > 0)
				player.QuickSpawnItem(player.GetSource_FromThis(), ModContent.ItemType<AugmentEssenceItem>(), refund);

			Refresh();
		}

		// Small "X" button in the panel's top-right corner. Same manual
		// hover/click approach as AugmentShopEntry's ActionButton, to match
		// this mod's existing UI button style.
		private class CloseButton : UIPanel
		{
			public event Action Clicked;

			private static readonly Color IdleColor = new Color(110, 40, 40);
			private static readonly Color HoverColor = new Color(160, 60, 60);

			public CloseButton()
			{
				SetPadding(0f);
				BackgroundColor = IdleColor;
				BorderColor = Color.White * 0.4f;

				UIText labelText = new UIText("x", 0.85f)
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
