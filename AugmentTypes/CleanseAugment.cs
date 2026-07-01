using Terraria;
using Terraria.ModLoader;

namespace Augments
{
	public class CleanseAugment : Augment
	{
		public override string Id => "cleanse";
		public override string DisplayName => "Cleanse";
		public override string Description =>
			$"{AugmentText.Active("Active:")} Press [Cleanse] to remove all debuffs from nearby teammates. {AugmentText.Cooldown("30s cooldown")}.";
		public override AugmentRarity Rarity => AugmentRarity.Rare;
		public override AugmentClass Class => AugmentClass.Support;
		public override bool HasAuraEffect => true;
		public override ModKeybind ActiveModKeybind => Augments.CleanseKeybind;
		public override int CooldownRemaining => Main.LocalPlayer.GetModPlayer<AugmentPlayer>().CleanseCooldown;
	}
}
