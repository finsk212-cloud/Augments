using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Augments
{
	public class AugmentUISystem : ModSystem
	{
		// --- Boss-kill choice popup ---
		private UserInterface augmentInterface;
		private AugmentChoiceUIState choiceState;

		// --- "Your Augments" browsable list ---
		private UserInterface listInterface;
		private AugmentListUIState listState;

		// --- Vendor shop panel ---
		private UserInterface shopInterface;
		private AugmentShopUIState shopState;

		private GameTime lastUpdateUiGameTime;

		public override void Load()
		{
			if (Main.dedServ)
				return; // dedicated server has no screen, skip UI setup entirely

			augmentInterface = new UserInterface();
			choiceState = new AugmentChoiceUIState();
			choiceState.Activate(); // forces OnInitialize now, so backPanel always exists

			listInterface = new UserInterface();
			listState = new AugmentListUIState();
			listState.Activate();

			shopInterface = new UserInterface();
			shopState = new AugmentShopUIState();
			shopState.Activate();
		}

		// Called every frame - keeps both panels' buttons/hover states responsive.
		public override void UpdateUI(GameTime gameTime)
		{
			lastUpdateUiGameTime = gameTime;

			if (augmentInterface?.CurrentState != null)
				augmentInterface.Update(gameTime);

			if (listInterface?.CurrentState != null)
				listInterface.Update(gameTime);

			if (shopInterface?.CurrentState != null)
				shopInterface.Update(gameTime);
		}

		// Slots both panels into Terraria's actual draw order.
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex == -1)
				return;

			// Insertion order matters here: each Insert places its layer
			// immediately before whatever currently sits at mouseTextIndex.
			// So the layer inserted FIRST in code ends up drawn LAST (on top).
			// We want: Auras (bottom) -> List UI -> Shop UI -> Choice UI -> Charges -> Cooldowns -> Tooltip (top) -> Mouse Text.
			// So in code: insert Tooltip first, ..., List UI, then Auras last.
			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"Augments: Tooltip",
				delegate
				{
					AugmentTooltipDrawer.DrawIfHovering(Main.spriteBatch);
					return true;
				},
				InterfaceScaleType.UI)
			);

			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"Augments: Cooldowns",
				delegate
				{
					AugmentCooldownDrawer.DrawCooldowns(Main.spriteBatch);
					return true;
				},
				InterfaceScaleType.UI)
			);

			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"Augments: Charges",
				delegate
				{
					AugmentChargeDrawer.DrawCharges(Main.spriteBatch);
					return true;
				},
				InterfaceScaleType.UI)
			);

			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"Augments: Choice UI",
				delegate
				{
					if (lastUpdateUiGameTime != null && augmentInterface?.CurrentState != null)
						augmentInterface.Draw(Main.spriteBatch, lastUpdateUiGameTime);
					return true;
				},
				InterfaceScaleType.UI)
			);

			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"Augments: Shop UI",
				delegate
				{
					if (lastUpdateUiGameTime != null && shopInterface?.CurrentState != null)
						shopInterface.Draw(Main.spriteBatch, lastUpdateUiGameTime);
					return true;
				},
				InterfaceScaleType.UI)
			);

			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"Augments: List UI",
				delegate
				{
					if (lastUpdateUiGameTime != null && listInterface?.CurrentState != null)
						listInterface.Draw(Main.spriteBatch, lastUpdateUiGameTime);
					return true;
				},
				InterfaceScaleType.UI)
			);

			// Inserted last = drawn first = beneath all UI panels but above the game world.
			// Uses InterfaceScaleType.Game so the spriteBatch gets the camera matrix
			// and AugmentAuraDrawer can pass world coordinates directly.
			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"Augments: Auras",
				delegate
				{
					AugmentAuraDrawer.DrawAuras(Main.spriteBatch);
					return true;
				},
				InterfaceScaleType.Game)
			);
		}

		// --- Choice popup controls ---

		public void ShowChoices(List<Augment> choices, AugmentRarity rarity)
		{
			choiceState.SetChoices(choices, rarity);
			augmentInterface?.SetState(choiceState);
		}

		public void HidePanel()
		{
			augmentInterface?.SetState(null);
		}

		public bool IsOpen => augmentInterface?.CurrentState != null;

		// --- "Your Augments" list controls ---

		public void ShowList()
		{
			listState.Refresh();
			listInterface?.SetState(listState);
		}

		public void HideList()
		{
			listInterface?.SetState(null);
		}

		public void ToggleList()
		{
			if (IsListOpen)
				HideList();
			else
				ShowList();
		}

		public bool IsListOpen => listInterface?.CurrentState != null;

		// --- Vendor shop panel controls ---

		public void ShowShop()
		{
			shopState.Refresh();
			shopInterface?.SetState(shopState);
		}

		public void HideShop()
		{
			shopInterface?.SetState(null);
		}

		public void ToggleShop()
		{
			if (IsShopOpen)
				HideShop();
			else
				ShowShop();
		}

		public bool IsShopOpen => shopInterface?.CurrentState != null;
	}
}