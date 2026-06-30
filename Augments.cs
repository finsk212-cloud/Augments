using Terraria.ModLoader;

namespace Augments
{
	public class Augments : Mod
	{
		public static ModKeybind OpenAugmentListKeybind;
		public static ModKeybind DebugTriggerPopupKeybind;
		public static ModKeybind DebugSpawnVendorKeybind;
		public static ModKeybind DebugToggleShopKeybind;

		public override void Load()
		{
			AugmentDatabase.Load();
			OpenAugmentListKeybind = KeybindLoader.RegisterKeybind(this, "OpenAugmentList", "OemOpenBrackets");
			DebugTriggerPopupKeybind = KeybindLoader.RegisterKeybind(this, "DebugTriggerPopup", "OemCloseBrackets");
			DebugSpawnVendorKeybind = KeybindLoader.RegisterKeybind(this, "DebugSpawnVendor", "OemSemicolon");
			DebugToggleShopKeybind = KeybindLoader.RegisterKeybind(this, "DebugToggleShop", "OemQuotes");

			// Registered manually, in this exact order, instead of relying on
			// autoload - AugmentFesteringWoundsNPC's UpdateLifeRegen must run
			// AFTER AugmentBleedNPC's so it amplifies a bleed contribution
			// that already landed this same tick. See the comments on both
			// classes for why autoload order can't be trusted for this.
			AddContent(new AugmentBleedNPC());
			AddContent(new AugmentFesteringWoundsNPC());
		}

		public override void Unload()
		{
			OpenAugmentListKeybind = null;
			DebugTriggerPopupKeybind = null;
			DebugSpawnVendorKeybind = null;
			DebugToggleShopKeybind = null;
		}
	}
}
