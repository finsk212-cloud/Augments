using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
	public static class AugmentRewardLogic
	{
		// Flat, one-shot chance (not per-slot) for an Epic/Legendary roll to
		// present a whole Keystone family instead of 3 normal picks.
		private const float KeystoneFamilyChance = 0.15f;

		// Rolls a rarity for the given bracket and pops the choice panel for
		// `player`. If the rolled rarity has nothing left to offer, steps
		// down one tier at a time until something is found (or Common also
		// comes up empty).
		public static void GrantReward(Player player, RarityBracket bracket)
		{
			AugmentRarity rolledRarity = BossRarityRoller.Roll(bracket);

			AugmentPlayer augmentPlayer = player.GetModPlayer<AugmentPlayer>();

			if (TryRollKeystoneFamily(augmentPlayer, rolledRarity, out List<Augment> keystoneChoices))
			{
				ShowRewardChoices(player, keystoneChoices, rolledRarity);
				return;
			}

			AugmentRarity actualRarity = rolledRarity;
			var choices = augmentPlayer.RollChoices(3, actualRarity);
			while (choices.Count == 0 && actualRarity > AugmentRarity.Common)
			{
				actualRarity--;
				choices = augmentPlayer.RollChoices(3, actualRarity);
			}

			if (choices.Count > 0)
				ShowRewardChoices(player, choices, actualRarity);
			else
				Main.NewText("No augments left to offer!", 255, 100, 100);
		}

		private static void ShowRewardChoices(Player player, List<Augment> choices, AugmentRarity rarity)
		{
			if (Main.netMode == NetmodeID.Server)
				AugmentNet.SendRewardChoices(player.whoAmI, choices, rarity);
			else
				ModContent.GetInstance<AugmentUISystem>().ShowChoices(choices, rarity);
		}

		// Only Epic/Legendary rolls get a shot at this - on success, an
		// entire unlocked Keystone family (all of its members, regardless of
		// each one's own exact rarity) replaces the popup outright, completely
		// bypassing AugmentPlayer.RollChoices for this kill. Keystones are
		// never mixed in 1-at-a-time with normal cards.
		private static bool TryRollKeystoneFamily(AugmentPlayer augmentPlayer, AugmentRarity rolledRarity, out List<Augment> choices)
		{
			choices = null;

			if (rolledRarity != AugmentRarity.Epic && rolledRarity != AugmentRarity.Legendary)
				return false;

			if (Main.rand.NextFloat() >= KeystoneFamilyChance)
				return false;

			var eligibleFamilies = new List<string>();
			foreach (var augment in AugmentDatabase.All)
			{
				if (augment.KeystoneFamily == null || eligibleFamilies.Contains(augment.KeystoneFamily))
					continue;

				if (augmentPlayer.LockedKeystoneFamilies.Contains(augment.KeystoneFamily))
					continue;

				bool hasMemberAtRolledRarity = false;
				foreach (var member in AugmentDatabase.All)
				{
					if (member.KeystoneFamily == augment.KeystoneFamily && member.Rarity == rolledRarity)
					{
						hasMemberAtRolledRarity = true;
						break;
					}
				}

				if (hasMemberAtRolledRarity)
					eligibleFamilies.Add(augment.KeystoneFamily);
			}

			if (eligibleFamilies.Count == 0)
				return false;

			string chosenFamily = eligibleFamilies[Main.rand.Next(eligibleFamilies.Count)];

			choices = new List<Augment>();
			foreach (var augment in AugmentDatabase.All)
			{
				if (augment.KeystoneFamily == chosenFamily)
					choices.Add(augment);
			}

			return true;
		}
	}
}
