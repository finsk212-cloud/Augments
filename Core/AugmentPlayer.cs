using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Augments
{
	public class AugmentPlayer : ModPlayer
	{
		public List<Augment> Owned = new List<Augment>();

		// Every augment Id this player has ever owned, even after later being
		// removed via RemoveAugment - this list only ever grows. Drives the
		// vendor shop's "Buy Back" section (ever-owned but not currently owned).
		public List<string> EverOwnedIds = new List<string>();

		// Keystone families with one member already chosen - this list only
		// ever grows too, same as EverOwnedIds, and is what permanently
		// excludes every sibling in that family from RollChoices once set.
		public List<string> LockedKeystoneFamilies = new List<string>();

		public bool HasAugment(string id)
		{
			foreach (var a in Owned)
			{
				if (a.Id == id)
					return true;
			}
			return false;
		}

		public void GrantAugment(Augment augment)
		{
			if (augment == null || HasAugment(augment.Id))
				return;

			Owned.Add(augment);
			if (!EverOwnedIds.Contains(augment.Id))
				EverOwnedIds.Add(augment.Id);

			if (augment.KeystoneFamily != null && !LockedKeystoneFamilies.Contains(augment.KeystoneFamily))
				LockedKeystoneFamilies.Add(augment.KeystoneFamily);

			augment.OnAcquire(Player);
			Main.NewText($"Augment acquired: {augment.DisplayName}", 255, 215, 0);
		}

		public void RemoveAugment(Augment augment)
		{
			if (augment == null)
				return;

			Owned.RemoveAll(a => a.Id == augment.Id);
		}

		// Live-summed, not separately saved - every owned augment's
		// FortuneBonus stacks additively into one shared Luck stat.
		public float TotalFortune => Owned.Sum(a => a.FortuneBonus);

		// Live count of Support-class augments owned. Player is "in Support
		// stance" when this is >= 2, which applies a damage penalty and defense
		// bonus that both scale with the count (see UpdateEquips below).
		public int SupportAugmentCount => Owned.Count(a => a.Class == AugmentClass.Support);

		// Shared pixel radius for all pull-based Support auras.
		private const float AuraRadius = 600f;

		// Set true each UpdateEquips tick when the Ironclad Aura defense bonus
		// was applied from a nearby Support player. Read by AugmentCooldownDrawer
		// to show the status icon. Reset to false at the top of each UpdateEquips.
		public bool ReceivedIroncladAura;

		// Picks up to `count` random augments of the given rarity that the
		// player doesn't already own, no repeats. With any TotalFortune,
		// each slot independently gets a 15% chance to specifically try for
		// a lucky-themed pick instead of a fully random one, falling back to
		// the normal random pick if no eligible lucky-themed augment exists.
		private const float LuckyThemedBiasChance = 0.15f;

		public List<Augment> RollChoices(int count, AugmentRarity rarity)
		{
			var available = new List<Augment>();
			foreach (var augment in AugmentDatabase.All)
			{
				if (augment.Rarity != rarity || augment.IsDebugOnly || HasAugment(augment.Id))
					continue;

				// Keystones never appear through this normal per-slot roll at
				// all, locked or not - the only way one is ever offered is the
				// separate whole-family check in AugmentRewardLogic.GrantReward,
				// which presents all of a family's members together as a set.
				if (augment.KeystoneFamily != null)
					continue;

				available.Add(augment);
			}

			bool hasFortune = TotalFortune > 0f;

			var picks = new List<Augment>();
			while (picks.Count < count && available.Count > 0)
			{
				int index = -1;

				if (hasFortune && Main.rand.NextFloat() < LuckyThemedBiasChance)
				{
					var luckyIndices = new List<int>();
					for (int i = 0; i < available.Count; i++)
					{
						if (available[i].IsLuckyThemed)
							luckyIndices.Add(i);
					}

					if (luckyIndices.Count > 0)
						index = luckyIndices[Main.rand.Next(luckyIndices.Count)];
				}

				if (index < 0)
					index = Main.rand.Next(available.Count);

				picks.Add(available[index]);
				available.RemoveAt(index);
			}
			return picks;
		}

		public override void PostUpdate()
		{
			foreach (var a in Owned)
				a.OnUpdate(Player);

			// Pull-based Warcry aura: each player checks nearby Support players
			// for the "warcry" augment and self-applies the buff. This is
			// multiplayer-safe because every client only modifies their own player.
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player other = Main.player[i];
				if (!other.active || other.dead || other == Player)
					continue;
				if (Vector2.Distance(Player.Center, other.Center) > AuraRadius)
					continue;
				var otherAP = other.GetModPlayer<AugmentPlayer>();
				if (otherAP.SupportAugmentCount < 2 || !otherAP.HasAugment("warcry"))
					continue;
				// 10-tick duration = refreshed each tick while in range, falls off
				// within 10 ticks (~0.17s) of the Support player leaving range.
				Player.AddBuff(ModContent.BuffType<WarCryBuff>(), 10);
				break;
			}

			// Owner also benefits from their own Warcry - the pull loop skips
			// self, so this handles the Support player's own application.
			if (SupportAugmentCount >= 2 && HasAugment("warcry"))
				Player.AddBuff(ModContent.BuffType<WarCryBuff>(), 10);
		}

		public override void PostUpdateRunSpeeds()
		{
			foreach (var a in Owned)
				a.PostUpdateRunSpeeds(Player);
		}

		public override void UpdateEquips()
		{
			ReceivedIroncladAura = false;

			foreach (var a in Owned)
				a.UpdateEquips(Player);

			int supportCount = SupportAugmentCount;
			if (supportCount >= 2)
			{
				float damagePenalty;
				int defenseBonus;

				if      (supportCount == 2) { damagePenalty = -0.30f; defenseBonus = 20; }
				else if (supportCount == 3) { damagePenalty = -0.23f; defenseBonus = 30; }
				else if (supportCount == 4) { damagePenalty = -0.16f; defenseBonus = 40; }
				else if (supportCount == 5) { damagePenalty = -0.10f; defenseBonus = 50; }
				else                        { damagePenalty = -0.05f; defenseBonus = 60; }

				Player.GetDamage(DamageClass.Generic) += damagePenalty;
				Player.statDefense += defenseBonus;
			}

			// Pull-based Ironclad Aura: each player checks nearby Support players
			// for the "ironclad_aura" augment and adds the defense bonus to themselves.
			// DESIGN FLAG: if two Support players both own this augment and both stand
			// near a teammate, that teammate receives +16 defense instead of +8.
			// Current behavior (break on first match) caps it at one source.
			// Remove the break to allow stacking from multiple Support players.
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player other = Main.player[i];
				if (!other.active || other.dead || other == Player)
					continue;
				if (Vector2.Distance(Player.Center, other.Center) > AuraRadius)
					continue;
				var otherAP = other.GetModPlayer<AugmentPlayer>();
				if (otherAP.SupportAugmentCount < 2 || !otherAP.HasAugment("ironclad_aura"))
					continue;
				Player.statDefense += 8;
				ReceivedIroncladAura = true;
				break;
			}

			// Owner also benefits from their own Ironclad Aura. The
			// !ReceivedIroncladAura guard prevents double-application if a
			// second Support player nearby already triggered the pull loop above.
			if (!ReceivedIroncladAura && SupportAugmentCount >= 2 && HasAugment("ironclad_aura"))
			{
				Player.statDefense += 8;
				ReceivedIroncladAura = true;
			}
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (Main.myPlayer != Player.whoAmI)
				return;

			if (Augments.OpenAugmentListKeybind.JustPressed)
				ModContent.GetInstance<AugmentUISystem>().ToggleList();

			// Debug: pop the reward choice UI without killing a boss. Endgame
			// is hardcoded since it's the bracket that can roll all 4
			// rarities via fallback, which is most useful for testing.
			if (Augments.DebugTriggerPopupKeybind.JustPressed)
				AugmentRewardLogic.GrantReward(Player, RarityBracket.Endgame);

			// Debug: force-spawn the vendor NPC at the player, bypassing
			// CanTownNPCSpawn entirely (that check only gates the automatic
			// town-relocation system, not a direct NewNPC call) - lets us
			// test the NPC without needing a house or Skeletron downed.
			if (Augments.DebugSpawnVendorKeybind.JustPressed)
			{
				NPC.NewNPC(Player.GetSource_FromThis(), (int)Player.position.X, (int)Player.position.Y, ModContent.NPCType<AugmentVendorNPC>());
				Main.NewText("Debug: spawn attempted for AugmentVendorNPC");
			}

			// Debug: open the vendor shop panel directly, ahead of it being
			// wired to the NPC's chat button - lets the panel be tested on
			// its own before that wiring happens.
			if (Augments.DebugToggleShopKeybind.JustPressed)
				ModContent.GetInstance<AugmentUISystem>().ToggleShop();
		}

		// Fires on every melee/weapon hit - dispatches to whichever owned
		// augments care about it (most won't override this and do nothing).
		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			foreach (var a in Owned)
				a.OnHitNPCWithItem(Player, item, target, hit, AugmentHitSource.NormalAttack);

			// Twin Strike (DuplicatesOnHitEffects) - a crit re-fires every
			// owned augment's on-hit reaction a second time. Deliberately
			// scoped to this dispatcher only; ModifyHitNPCWithItem's flat
			// damage-bonus modifiers below are untouched, single pass always.
			if (hit.Crit && Owned.Any(a => a.DuplicatesOnHitEffects))
			{
				foreach (var a in Owned)
					a.OnHitNPCWithItem(Player, item, target, hit, AugmentHitSource.NormalAttack);
			}
		}

		// Covers projectile hits, which includes thrust-style melee weapons
		// like short swords and spears - they register hits this way instead
		// of through OnHitNPCWithItem.
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			AugmentProjectileTag tag = proj.GetGlobalProjectile<AugmentProjectileTag>();
			if (tag.IsAugmentProcDamage && !tag.CanTriggerOnHitAugments)
				return;

			AugmentHitSource source = tag.IsAugmentProcDamage ? AugmentHitSource.AugmentProc : AugmentHitSource.NormalAttack;
			float effectiveness = tag.IsAugmentProcDamage ? MathHelper.Clamp(tag.OnHitEffectiveness, 0f, 1f) : 1f;

			foreach (var a in Owned)
				a.OnHitNPCWithProj(Player, proj, target, hit, source, effectiveness);

			// Mirrors the OnHitNPCWithItem second pass above.
			if (source == AugmentHitSource.NormalAttack && hit.Crit && Owned.Any(a => a.DuplicatesOnHitEffects))
			{
				foreach (var a in Owned)
					a.OnHitNPCWithProj(Player, proj, target, hit, source, effectiveness);
			}
		}

		// Fires BEFORE a melee hit is finalized - lets augments boost the
		// actual damage of the hit via modifiers.FlatBonusDamage.
		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
		{
			foreach (var a in Owned)
				a.ModifyHitNPCWithItem(Player, item, target, ref modifiers, AugmentHitSource.NormalAttack);
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			AugmentProjectileTag tag = proj.GetGlobalProjectile<AugmentProjectileTag>();
			if (tag.IsAugmentProcDamage && !tag.CanTriggerOnHitAugments)
				return;

			AugmentHitSource source = tag.IsAugmentProcDamage ? AugmentHitSource.AugmentProc : AugmentHitSource.NormalAttack;
			float effectiveness = tag.IsAugmentProcDamage ? MathHelper.Clamp(tag.OnHitEffectiveness, 0f, 1f) : 1f;

			foreach (var a in Owned)
				a.ModifyHitNPCWithProj(Player, proj, target, ref modifiers, source, effectiveness);
		}

		public override bool FreeDodge(Player.HurtInfo info)
		{
			foreach (var a in Owned)
			{
				if (a.FreeDodge(Player, info))
					return true;
			}
			return false;
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			foreach (var a in Owned)
				a.ModifyHurt(Player, ref modifiers);
		}

		public override void OnHurt(Player.HurtInfo info)
		{
			foreach (var a in Owned)
				a.OnHurt(Player, info);
		}

		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
		{
			foreach (var a in Owned)
				a.OnHitByNPC(Player, npc, hurtInfo);
		}

		public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
		{
			foreach (var a in Owned)
				a.ModifyManaCost(Player, item, ref reduce, ref mult);
		}

		public override void OnConsumeMana(Item item, int manaConsumed)
		{
			foreach (var a in Owned)
				a.OnConsumeMana(Player, item, manaConsumed);
		}

		public override bool CanConsumeAmmo(Item weapon, Item ammo)
		{
			foreach (var a in Owned)
			{
				if (!a.CanConsumeAmmo(Player, weapon, ammo))
					return false;
			}
			return true;
		}

		public override void ModifyWeaponCrit(Item item, ref float crit)
		{
			foreach (var a in Owned)
				a.ModifyWeaponCrit(Player, item, ref crit);
		}

		public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
		{
			foreach (var a in Owned)
				a.GetHealLife(Player, item, quickHeal, ref healValue);
		}

		public override void ModifyFishingAttempt(ref FishingAttempt attempt)
		{
			foreach (var a in Owned)
				a.ModifyFishingAttempt(Player, ref attempt);
		}

		public override void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel)
		{
			foreach (var a in Owned)
				a.GetFishingLevel(Player, fishingRod, bait, ref fishingLevel);
		}

		// --- Persistence: augments survive between play sessions ---

		public override void SaveData(TagCompound tag)
		{
			var ids = new List<string>();
			var customData = new TagCompound();
			foreach (var a in Owned)
			{
				ids.Add(a.Id);

				var augmentTag = new TagCompound();
				a.SaveCustomData(augmentTag);
				if (augmentTag.Count > 0)
					customData[a.Id] = augmentTag;
			}
			tag["augmentIds"] = ids;
			tag["augmentCustomData"] = customData;
			tag["everOwnedIds"] = EverOwnedIds;
			tag["lockedKeystoneFamilies"] = LockedKeystoneFamilies;
		}

		public override void LoadData(TagCompound tag)
		{
			Owned.Clear();
			if (tag.ContainsKey("augmentIds"))
			{
				foreach (var id in tag.GetList<string>("augmentIds"))
				{
					var augment = AugmentDatabase.GetById(id);
					if (augment != null)
						Owned.Add(augment);
				}
			}

			if (tag.GetCompound("augmentCustomData") is TagCompound customData)
			{
				foreach (var a in Owned)
				{
					if (customData.GetCompound(a.Id) is TagCompound augmentTag)
						a.LoadCustomData(augmentTag);
				}
			}

			EverOwnedIds.Clear();
			if (tag.ContainsKey("everOwnedIds"))
				EverOwnedIds.AddRange(tag.GetList<string>("everOwnedIds"));

			// Saves predate EverOwnedIds tracking - backfill from whatever's
			// currently owned so pre-existing augments aren't invisible to
			// "Buy Back" the first time they're later removed.
			foreach (var a in Owned)
			{
				if (!EverOwnedIds.Contains(a.Id))
					EverOwnedIds.Add(a.Id);
			}

			LockedKeystoneFamilies.Clear();
			if (tag.ContainsKey("lockedKeystoneFamilies"))
				LockedKeystoneFamilies.AddRange(tag.GetList<string>("lockedKeystoneFamilies"));

			// Saves predate Keystone tracking too - backfill from whatever
			// Keystone is currently owned so a save made before this feature
			// existed still correctly locks out that Keystone's siblings.
			foreach (var a in Owned)
			{
				if (a.KeystoneFamily != null && !LockedKeystoneFamilies.Contains(a.KeystoneFamily))
					LockedKeystoneFamilies.Add(a.KeystoneFamily);
			}
		}
	}
}
