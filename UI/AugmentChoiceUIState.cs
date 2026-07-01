using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace Augments
{
	public class AugmentChoiceUIState : UIState
	{
		private UIPanel backPanel;
		private readonly List<AugmentChoiceCard> cards = new List<AugmentChoiceCard>();
		private RerollButton rerollButton;
		private UIText capNoticeText;

		// Rarity tier this popup's cards were rolled at - a reroll must stay
		// on this same tier, not re-roll a fresh (possibly different) one.
		private AugmentRarity currentRarity;

		// Exactly one reroll per popup, no matter how much Essence is
		// stockpiled - reset to false only when a fresh popup is shown.
		private bool rerollUsed;
		private bool networkReward;

		// --- Keystone confirmation overlay ---
		// Appended/removed from `this` (the root UIState, not backPanel) so it
		// draws on top of and blocks clicks to everything behind it - cards,
		// reroll button, all of it - without needing a whole new interface
		// layer. It's still just part of the same "Augments: Choice UI" layer
		// AugmentUISystem already draws this state through.
		private UIPanel confirmOverlay;
		private UIText confirmMessageText;
		private Augment pendingKeystone;

		private const float PanelWidth = 860f;
		private const float CardWidth = 270f;
		private const float CardSpacing = 14f;
		private const float CardsTop = 68f;
		private const float BottomMargin = 24f;

		// Dedicated strip below the cards for the reroll button - kept as its
		// own gap + button height rather than folded into BottomMargin, so the
		// button always has clear, non-overlapping space under the card row.
		private const float RerollGap = 20f;
		private const float RerollButtonHeight = 34f;

		private const float ConfirmBoxWidth = 520f;
		private const float ConfirmBoxHeight = 260f;
		private const float ConfirmButtonWidth = 180f;
		private const float ConfirmButtonHeight = 36f;

		public override void OnInitialize()
		{
			backPanel = new UIPanel();
			backPanel.Width.Set(PanelWidth, 0f);
			// Driven by AugmentChoiceCard.MinCardHeight (the cards' actual fixed
			// height) rather than a separate hardcoded number, plus room for the
			// reroll strip below the cards.
			backPanel.Height.Set(CardsTop + AugmentChoiceCard.MinCardHeight + RerollGap + RerollButtonHeight + BottomMargin, 0f);
			backPanel.HAlign = 0.5f;
			backPanel.VAlign = 0.5f;
			backPanel.BackgroundColor = new Color(33, 43, 79) * 0.9f;

			UIText titleText = new UIText("Choose an augment", 1.2f)
			{
				HAlign = 0.5f,
				TextColor = new Color(255, 170, 40)
			};
			titleText.Top.Set(15f, 0f);
			backPanel.Append(titleText);

			capNoticeText = new UIText("", 0.8f)
			{
				HAlign = 0.5f,
				TextColor = AugmentTextColors.Cooldown
			};
			capNoticeText.Top.Set(42f, 0f);
			backPanel.Append(capNoticeText);

			rerollButton = new RerollButton();
			rerollButton.Width.Set(200f, 0f);
			rerollButton.Height.Set(RerollButtonHeight, 0f);
			rerollButton.HAlign = 0.5f;
			rerollButton.Top.Set(CardsTop + AugmentChoiceCard.MinCardHeight + RerollGap, 0f);
			rerollButton.Clicked += HandleRerollClicked;
			backPanel.Append(rerollButton);

			Append(backPanel);

			BuildConfirmOverlay();
		}

		// Built once and kept around, but only appended to the root UIState
		// (and so only drawn/clickable) while a Keystone pick is actually
		// pending confirmation.
		private void BuildConfirmOverlay()
		{
			confirmOverlay = new UIPanel();
			confirmOverlay.Width.Set(0f, 1f);
			confirmOverlay.Height.Set(0f, 1f);
			confirmOverlay.SetPadding(0f);
			confirmOverlay.BackgroundColor = Color.Black * 0.6f;
			confirmOverlay.BorderColor = Color.Transparent;

			var confirmBox = new UIPanel();
			confirmBox.Width.Set(ConfirmBoxWidth, 0f);
			confirmBox.Height.Set(ConfirmBoxHeight, 0f);
			confirmBox.HAlign = 0.5f;
			confirmBox.VAlign = 0.5f;
			confirmBox.BackgroundColor = new Color(33, 43, 79) * 0.95f;
			confirmBox.BorderColor = new Color(220, 60, 60);
			confirmOverlay.Append(confirmBox);

			UIText confirmTitleText = new UIText("Permanent Choice", 1.05f)
			{
				HAlign = 0.5f,
				TextColor = new Color(220, 60, 60)
			};
			confirmTitleText.Top.Set(14f, 0f);
			confirmBox.Append(confirmTitleText);

			confirmMessageText = new UIText("", 0.8f)
			{
				HAlign = 0.5f,
				IsWrapped = true
			};
			confirmMessageText.Width.Set(-30f, 1f);
			confirmMessageText.Height.Set(120f, 0f);
			confirmMessageText.Top.Set(50f, 0f);
			confirmBox.Append(confirmMessageText);

			var confirmButton = new ModalButton("Confirm", new Color(110, 40, 40), new Color(160, 60, 60));
			confirmButton.Width.Set(ConfirmButtonWidth, 0f);
			confirmButton.Height.Set(ConfirmButtonHeight, 0f);
			confirmButton.HAlign = 0.25f;
			confirmButton.VAlign = 1f;
			confirmButton.Top.Set(-16f, 0f);
			confirmButton.Clicked += HandleKeystoneConfirmed;
			confirmBox.Append(confirmButton);

			var cancelButton = new ModalButton("Cancel", new Color(60, 70, 110), new Color(90, 105, 160));
			cancelButton.Width.Set(ConfirmButtonWidth, 0f);
			cancelButton.Height.Set(ConfirmButtonHeight, 0f);
			cancelButton.HAlign = 0.75f;
			cancelButton.VAlign = 1f;
			cancelButton.Top.Set(-16f, 0f);
			cancelButton.Clicked += HandleKeystoneCanceled;
			confirmBox.Append(cancelButton);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			RefreshRerollButton();
		}

		// Call this right before showing the panel - replaces whatever cards
		// were there with a fresh set built from the given augments, and resets
		// this popup's reroll allowance.
		public void SetChoices(List<Augment> choices, AugmentRarity rarity, bool networkReward = false)
		{
			currentRarity = rarity;
			rerollUsed = false;
			this.networkReward = networkReward;
			HideKeystoneConfirm();

			RebuildCards(choices);
			RefreshCapNotice();
			RefreshRerollButton();
		}

		private void RebuildCards(List<Augment> choices)
		{
			foreach (var card in cards)
				backPanel.RemoveChild(card);
			cards.Clear();

			int count = choices.Count;
			// Center against the panel's actual current inner width (post-padding),
			// not the raw PanelWidth constant - children are positioned relative to
			// GetInnerDimensions(), so centering against the outer width leaves the
			// row off-center by the panel's padding.
			float panelInnerWidth = backPanel.GetInnerDimensions().Width;
			float totalWidth = count * CardWidth + (count - 1) * CardSpacing;
			float startX = (panelInnerWidth - totalWidth) / 2f;

			for (int i = 0; i < count; i++)
			{
				var card = new AugmentChoiceCard(choices[i], CardWidth);
				card.Left.Set(startX + i * (CardWidth + CardSpacing), 0f);
				card.Top.Set(CardsTop, 0f);
				card.OnAugmentChosen += HandleAugmentChosen;

				backPanel.Append(card);
				cards.Add(card);
			}
		}

		// Keystone picks need confirmation first (see ShowKeystoneConfirm) since
		// they permanently lock their family's siblings out of every future
		// popup - everything else still grants immediately, exactly as before.
		private void HandleAugmentChosen(Augment augment)
		{
			if (augment.KeystoneFamily != null)
				ShowKeystoneConfirm(augment);
			else
				GrantAndClose(augment);
		}

		private void ShowKeystoneConfirm(Augment augment)
		{
			pendingKeystone = augment;

			var siblingNames = new List<string>();
			foreach (var other in AugmentDatabase.All)
			{
				if (other.KeystoneFamily == augment.KeystoneFamily && other.Id != augment.Id)
					siblingNames.Add(other.DisplayName);
			}

			confirmMessageText.SetText(
				$"Choosing {augment.DisplayName} will permanently exclude {JoinWithAnd(siblingNames)} " +
				"from ever being offered to you again. This cannot be undone.");

			Append(confirmOverlay);
		}

		private void HideKeystoneConfirm()
		{
			pendingKeystone = null;
			RemoveChild(confirmOverlay);
		}

		private void HandleKeystoneConfirmed()
		{
			if (pendingKeystone == null)
				return;

			GrantAndClose(pendingKeystone);
		}

		// Dismisses the confirmation and returns to the original 3-card
		// choice without granting anything - the cards underneath were never
		// touched, so a different card can still be picked instead.
		private void HandleKeystoneCanceled()
		{
			HideKeystoneConfirm();
		}

		private void GrantAndClose(Augment augment)
		{
			var player = Main.LocalPlayer;
			var ap = player.GetModPlayer<AugmentPlayer>();

			ap.ChooseReward(augment);

			pendingKeystone = null;
			RemoveChild(confirmOverlay);
			ModContent.GetInstance<AugmentUISystem>().HidePanel();
		}

		private void RefreshCapNotice()
		{
			var ap = Main.LocalPlayer.GetModPlayer<AugmentPlayer>();
			capNoticeText.SetText(ap.Owned.Count >= AugmentPlayer.MaxOwnedAugments
				? "Augment slots full - selection will be sold to Mommy 2B."
				: "");
		}

		private static string JoinWithAnd(List<string> names)
		{
			if (names.Count == 0)
				return "";
			if (names.Count == 1)
				return names[0];

			return string.Join(", ", names.GetRange(0, names.Count - 1)) + " and " + names[names.Count - 1];
		}

		private void HandleRerollClicked()
		{
			// Hard gate - this is re-checked here regardless of the button's
			// own visual enabled state, since this bool is the entire point
			// of the feature and must hold even with a large Essence stockpile.
			if (networkReward)
				return;

			if (rerollUsed)
				return;

			var player = Main.LocalPlayer;
			int essenceType = ModContent.ItemType<AugmentEssenceItem>();

			if (player.CountItem(essenceType, 1) < 1)
			{
				Main.NewText("Not enough Augment Essence.", 255, 80, 80);
				return;
			}

			player.ConsumeItem(essenceType);
			rerollUsed = true;

			var newChoices = player.GetModPlayer<AugmentPlayer>().RollChoices(3, currentRarity);
			RebuildCards(newChoices);
			RefreshRerollButton();
		}

		private void RefreshRerollButton()
		{
			if (networkReward)
			{
				rerollButton.SetEnabled(false, "Server Reward");
				return;
			}

			if (rerollUsed)
			{
				rerollButton.SetEnabled(false, "Reroll Used");
				return;
			}

			int essenceCount = Main.LocalPlayer.CountItem(ModContent.ItemType<AugmentEssenceItem>());
			rerollButton.SetEnabled(essenceCount >= 1, "Reroll (1 Essence)");
		}

		// Small standalone clickable panel - same manual hover/click approach
		// AugmentChoiceCard/AugmentShopEntry use, to match this mod's existing
		// UI button style. "Disabled" here is just a grey visual hint - the
		// real gating lives in AugmentChoiceUIState.HandleRerollClicked.
		private class RerollButton : UIPanel
		{
			public event Action Clicked;

			private static readonly Color IdleColor = new Color(60, 70, 110);
			private static readonly Color HoverColor = new Color(90, 105, 160);
			private static readonly Color DisabledColor = new Color(45, 45, 45);

			private readonly UIText labelText;
			private bool enabledState = true;

			public RerollButton()
			{
				SetPadding(0f);
				BackgroundColor = IdleColor;
				BorderColor = Color.White * 0.4f;

				labelText = new UIText("Reroll (1 Essence)", 0.8f)
				{
					HAlign = 0.5f,
					VAlign = 0.5f
				};
				Append(labelText);
			}

			public void SetEnabled(bool enabled, string label)
			{
				labelText.SetText(label);

				if (enabledState == enabled)
					return;

				enabledState = enabled;
				BackgroundColor = enabled ? IdleColor : DisabledColor;
			}

			public override void LeftClick(UIMouseEvent evt)
			{
				base.LeftClick(evt);
				// Always fires - HandleRerollClicked does the real rerollUsed/Essence
				// checks and shows the insufficient-funds message itself; the grey
				// styling here is just a visual hint, not the actual gate.
				Clicked?.Invoke();
			}

			public override void MouseOver(UIMouseEvent evt)
			{
				base.MouseOver(evt);
				if (enabledState)
					BackgroundColor = HoverColor;
			}

			public override void MouseOut(UIMouseEvent evt)
			{
				base.MouseOut(evt);
				if (enabledState)
					BackgroundColor = IdleColor;
			}
		}

		// Generic small clickable panel for the keystone confirm overlay's
		// Confirm/Cancel buttons - same manual hover/click approach as
		// RerollButton, just without a disabled state since both buttons are
		// always actionable for as long as the overlay is shown.
		private class ModalButton : UIPanel
		{
			public event Action Clicked;

			private readonly Color idleColor;
			private readonly Color hoverColor;

			public ModalButton(string label, Color idleColor, Color hoverColor)
			{
				this.idleColor = idleColor;
				this.hoverColor = hoverColor;

				SetPadding(0f);
				BackgroundColor = idleColor;
				BorderColor = Color.White * 0.4f;

				UIText labelText = new UIText(label, 0.85f)
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
				BackgroundColor = hoverColor;
			}

			public override void MouseOut(UIMouseEvent evt)
			{
				base.MouseOut(evt);
				BackgroundColor = idleColor;
			}
		}
	}
}
