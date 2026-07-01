using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Augments
{
	public class AugmentPlayer : ModPlayer
	{
		public const int MaxOwnedAugments = 5;

		private readonly HashSet<string> ownedIds = new HashSet<string>();
		private readonly HashSet<string> everOwnedIds = new HashSet<string>();
		private readonly HashSet<string> soldAugmentIds = new HashSet<string>();
		private readonly HashSet<string> lockedKeystoneFamilies = new HashSet<string>();
		private readonly List<Augment> owned = new List<Augment>();

		public IReadOnlySet<string> OwnedIds => ownedIds;
		public IReadOnlySet<string> EverOwnedIds => everOwnedIds;
		public IReadOnlySet<string> SoldAugmentIds => soldAugmentIds;
		public IReadOnlySet<string> LockedKeystoneFamilies => lockedKeystoneFamilies;
		public IReadOnlyList<Augment> Owned => owned;

		// How many times each boss type has granted (or attempted to grant)
		// augment selection to this player. Persisted across sessions.
		// Key = NPC.type, value = attempt count (increments even on failed rolls).
		public Dictionary<int, int> BossAugmentKills = new Dictionary<int, int>();

		// Boss types the player has dealt damage to during the current play session.
		// Session-only — resets on world enter. Used for multiplayer participation
		// checks (skipped in singleplayer; see TODO in BossAugmentDrop).
		public HashSet<int> DamagedBossesThisFight = new HashSet<int>();

		// Keystone families with one member already chosen permanently exclude
		// every sibling in that family from RollChoices once set.

		public bool HasAugment(string id)
		{
			return id != null && ownedIds.Contains(id);
		}

		public bool GrantAugmentByIdServerAuthoritative(string id, bool sync = true)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return false;

			Augment augment = AugmentDatabase.GetById(id);
			if (augment == null || ownedIds.Contains(id) || soldAugmentIds.Contains(id) || ownedIds.Count >= MaxOwnedAugments)
				return false;

			ownedIds.Add(id);
			everOwnedIds.Add(id);
			if (augment.KeystoneFamily != null)
				lockedKeystoneFamilies.Add(augment.KeystoneFamily);

			RebuildOwnedCacheFromOwnedIdsOnly();
			augment.OnAcquire(Player);
			if (Main.netMode != NetmodeID.Server)
				Main.NewText($"Augment acquired: {augment.DisplayName}", 255, 215, 0);

			DebugCheckState("GrantAugmentByIdServerAuthoritative");
			if (sync && Main.netMode == NetmodeID.Server)
				AugmentNet.SendSyncPlayer(Player);
			return true;
		}

		public bool RemoveAugmentByIdServerAuthoritative(string id, bool sync = true)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || !ownedIds.Remove(id))
				return false;

			RebuildOwnedCacheFromOwnedIdsOnly();
			DebugCheckState("RemoveAugmentByIdServerAuthoritative");
			if (sync && Main.netMode == NetmodeID.Server)
				AugmentNet.SendSyncPlayer(Player);
			return true;
		}

		public bool SellAugmentByIdServerAuthoritative(string id, bool sync = true)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || !ownedIds.Remove(id))
				return false;

			Augment augment = AugmentDatabase.GetById(id);
			if (augment == null)
			{
				ownedIds.Add(id);
				return false;
			}

			soldAugmentIds.Add(id);
			everOwnedIds.Add(id);
			RebuildOwnedCacheFromOwnedIdsOnly();

			int refund = GetRemoveRefund(augment.Rarity);
			if (refund > 0)
				SpawnEssenceRefund(refund);

			DebugCheckState("SellAugmentByIdServerAuthoritative");
			if (sync && Main.netMode == NetmodeID.Server)
				AugmentNet.SendSyncPlayer(Player);
			return true;
		}

		public bool BuyBackSoldAugmentByIdServerAuthoritative(string id, bool sync = true)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || !soldAugmentIds.Contains(id) || ownedIds.Count >= MaxOwnedAugments)
				return false;

			Augment augment = AugmentDatabase.GetById(id);
			if (augment == null)
				return false;

			int cost = GetBuyBackCost(augment.Rarity);
			int essenceType = ModContent.ItemType<AugmentEssenceItem>();
			if (Player.CountItem(essenceType, cost) < cost)
				return false;

			for (int i = 0; i < cost; i++)
				Player.ConsumeItem(essenceType);

			soldAugmentIds.Remove(id);
			ownedIds.Add(id);
			everOwnedIds.Add(id);
			RebuildOwnedCacheFromOwnedIdsOnly();
			augment.OnAcquire(Player);

			if (Main.netMode == NetmodeID.Server)
				SyncInventory();

			DebugCheckState("BuyBackSoldAugmentByIdServerAuthoritative");
			if (sync && Main.netMode == NetmodeID.Server)
				AugmentNet.SendSyncPlayer(Player);
			return true;
		}

		public void ChooseReward(Augment augment)
		{
			if (augment == null)
				return;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				AugmentNet.SendChooseReward(augment.Id);
				return;
			}

			ApplyRewardAugment(augment);
		}

		public bool ApplyRewardAugment(Augment augment, bool sync = true)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient || augment == null || HasAugment(augment.Id) || soldAugmentIds.Contains(augment.Id))
				return false;

			if (ownedIds.Count >= MaxOwnedAugments)
			{
				everOwnedIds.Add(augment.Id);
				soldAugmentIds.Add(augment.Id);

				int refund = GetRewardRefund(augment.Rarity);
				if (refund > 0)
					SpawnEssenceRefund(refund);

				if (Main.netMode != NetmodeID.Server)
				{
					string msg = refund > 0
						? $"{augment.DisplayName} sold to Mommy 2B - received {refund} Augment Essence."
						: $"{augment.DisplayName} sold to Mommy 2B. Available to buy back later.";
					Main.NewText(msg, 180, 220, 255);
				}

				DebugCheckState("ApplyRewardAugmentSoldAtCap");
				if (sync && Main.netMode == NetmodeID.Server)
					AugmentNet.SendSyncPlayer(Player);

				return true;
			}

			return GrantAugmentByIdServerAuthoritative(augment.Id, sync);
		}

		private static int GetRewardRefund(AugmentRarity rarity)
		{
			switch (rarity)
			{
				case AugmentRarity.Epic:
					return 1;
				case AugmentRarity.Legendary:
					return 2;
				default:
					return 0;
			}
		}

		private void SpawnEssenceRefund(int amount)
		{
			if (amount <= 0 || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			int itemIndex = Item.NewItem(Player.GetSource_FromThis(), Player.Hitbox, ModContent.ItemType<AugmentEssenceItem>(), amount);
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);
		}

		public static int GetRemoveRefund(AugmentRarity rarity)
		{
			return rarity switch
			{
				AugmentRarity.Epic => 1,
				AugmentRarity.Legendary => 2,
				_ => 0
			};
		}

		public static int GetBuyBackCost(AugmentRarity rarity)
		{
			return rarity switch
			{
				AugmentRarity.Epic => 2,
				AugmentRarity.Legendary => 4,
				_ => 1
			};
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

		// Per-frame tracking for other pull-based aura effects.
		// Reset and re-evaluated each frame; used to prevent double-application
		// when two Support players both own the same aura augment.
		public bool ReceivedSwiftnessAura;
		public bool ReceivedManaWell;
		public bool ReceivedCombatMedic;

		// Per-player cooldowns for triggered Support auras (Last Rites, Lifeline).
		// Volatile — not saved to disk. 90s = 5400 ticks at 60 ticks/sec.
		public int LastRitesCooldown;
		private int lastRitesInvulnTicks;
		public int LifelineCooldown;
		private int lifelineInvulnTicks;

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
				if (soldAugmentIds.Contains(augment.Id))
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

			// Keep the Support Class buff active while any Support augment is owned.
			// Short duration refreshed every tick — expires within 3 frames if removed.
			if (SupportAugmentCount >= 1)
				Player.AddBuff(ModContent.BuffType<SupportClassBuff>(), 3);

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
				if (!otherAP.HasAugment("warcry"))
					continue;
				// 10-tick duration = refreshed each tick while in range, falls off
				// within 10 ticks (~0.17s) of the Support player leaving range.
				Player.AddBuff(ModContent.BuffType<WarCryBuff>(), 10);
				break;
			}

			// Owner also benefits from their own Warcry - the pull loop skips
			// self, so this handles the Support player's own application.
			if (HasAugment("warcry"))
				Player.AddBuff(ModContent.BuffType<WarCryBuff>(), 10);

			// Tick down triggered-aura cooldowns and re-assert invulnerability
			// each frame while it's active. Re-assertion is required because vanilla
			// overwrites player.immuneTime with its own short post-hit window every
			// tick — a one-shot set doesn't survive. See PhoenixHeartAugment for detail.
			if (LastRitesCooldown > 0) LastRitesCooldown--;
			if (lastRitesInvulnTicks > 0)
			{
				Player.immune = true;
				Player.immuneTime = lastRitesInvulnTicks;
				lastRitesInvulnTicks--;
			}

			if (LifelineCooldown > 0) LifelineCooldown--;
			if (lifelineInvulnTicks > 0)
			{
				Player.immune = true;
				Player.immuneTime = lifelineInvulnTicks;
				lifelineInvulnTicks--;
			}

			// Last Rites pull check: fires on the local player's own client when
			// they drop below 20% HP and no cooldown is active. Finds a nearby
			// Support player with the augment and grants 3s invulnerability.
			if (Player.statLife <= (int)(Player.statLifeMax2 * 0.20f) && LastRitesCooldown == 0)
			{
				for (int i = 0; i < Main.maxPlayers; i++)
				{
					Player other = Main.player[i];
					if (!other.active || other.dead || other == Player)
						continue;
					if (Vector2.Distance(Player.Center, other.Center) > AuraRadius)
						continue;
					var otherAP = other.GetModPlayer<AugmentPlayer>();
					if (!otherAP.HasAugment("last_rites"))
						continue;
					LastRitesCooldown = 5400;
					lastRitesInvulnTicks = 180;
					Player.AddBuff(ModContent.BuffType<LastRitesCooldownBuff>(), 5400);
					break;
				}
			}
		}

		public override void PostUpdateRunSpeeds()
		{
			ReceivedSwiftnessAura = false;

			foreach (var a in Owned)
				a.PostUpdateRunSpeeds(Player);

			// Pull-based Swiftness Aura: each player checks nearby Support players
			// for the "swiftness_aura" augment and self-applies the movement boost.
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player other = Main.player[i];
				if (!other.active || other.dead || other == Player)
					continue;
				if (Vector2.Distance(Player.Center, other.Center) > AuraRadius)
					continue;
				var otherAP = other.GetModPlayer<AugmentPlayer>();
				if (!otherAP.HasAugment("swiftness_aura"))
					continue;
				Player.maxRunSpeed *= 1.15f;
				Player.accRunSpeed *= 1.15f;
				Player.runAcceleration *= 1.15f;
				ReceivedSwiftnessAura = true;
				break;
			}

			// Owner also benefits from their own Swiftness Aura.
			if (!ReceivedSwiftnessAura && HasAugment("swiftness_aura"))
			{
				Player.maxRunSpeed *= 1.15f;
				Player.accRunSpeed *= 1.15f;
				Player.runAcceleration *= 1.15f;
				ReceivedSwiftnessAura = true;
			}
		}

		public override void UpdateEquips()
		{
			ReceivedIroncladAura = false;
			ReceivedManaWell = false;
			ReceivedCombatMedic = false;

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
				if (!otherAP.HasAugment("ironclad_aura"))
					continue;
				Player.statDefense += 8;
				ReceivedIroncladAura = true;
				break;
			}

			// Owner also benefits from their own Ironclad Aura. The
			// !ReceivedIroncladAura guard prevents double-application if a
			// second Support player nearby already triggered the pull loop above.
			if (!ReceivedIroncladAura && HasAugment("ironclad_aura"))
			{
				Player.statDefense += 8;
				ReceivedIroncladAura = true;
			}

			// Pull-based Mana Well: advance the mana regen timer by 30% each frame
			// when a nearby Support player owns the augment, making mana regenerate
			// approximately 30% faster. Player.manaRegen is a timer that counts up
			// to Player.manaRegenDelay; advancing it extra each tick shortens the cycle.
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player other = Main.player[i];
				if (!other.active || other.dead || other == Player)
					continue;
				if (Vector2.Distance(Player.Center, other.Center) > AuraRadius)
					continue;
				var otherAP = other.GetModPlayer<AugmentPlayer>();
				if (!otherAP.HasAugment("mana_well"))
					continue;
				Player.manaRegen += (int)(Player.manaRegen * 0.30f);
				ReceivedManaWell = true;
				break;
			}

			// Owner also benefits from their own Mana Well.
			if (!ReceivedManaWell && HasAugment("mana_well"))
			{
				Player.manaRegen += (int)(Player.manaRegen * 0.30f);
				ReceivedManaWell = true;
			}

			// Pull-based Combat Medic: +6 lifeRegen = 3 HP/sec (lifeRegen applies at
			// half its value per second — same field used by vanilla regen buffs/potions).
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player other = Main.player[i];
				if (!other.active || other.dead || other == Player)
					continue;
				if (Vector2.Distance(Player.Center, other.Center) > AuraRadius)
					continue;
				var otherAP = other.GetModPlayer<AugmentPlayer>();
				if (!otherAP.HasAugment("combat_medic"))
					continue;
				Player.lifeRegen += 6;
				ReceivedCombatMedic = true;
				break;
			}

			// Owner also benefits from their own Combat Medic.
			if (!ReceivedCombatMedic && HasAugment("combat_medic"))
			{
				Player.lifeRegen += 6;
				ReceivedCombatMedic = true;
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
			{
				if (Main.netMode == NetmodeID.SinglePlayer)
					AugmentRewardLogic.GrantReward(Player, RarityBracket.Endgame);
				else if (Main.netMode == NetmodeID.MultiplayerClient)
					AugmentNet.SendDebugRewardRequest();
			}

			// Debug: force-spawn the vendor NPC at the player, bypassing
			// CanTownNPCSpawn entirely (that check only gates the automatic
			// town-relocation system, not a direct NewNPC call) - lets us
			// test the NPC without needing a house or Skeletron downed.
			if (Augments.DebugSpawnVendorKeybind.JustPressed)
			{
				AugmentNet.RequestVendorSpawn(Player);
				Main.NewText("Debug: vendor spawn requested");
			}

			// Debug: open the vendor shop panel directly, ahead of it being
			// wired to the NPC's chat button - lets the panel be tested on
			// its own before that wiring happens.
			if (Augments.DebugToggleShopKeybind.JustPressed)
				ModContent.GetInstance<AugmentUISystem>().ToggleShop();
		}

		// Fires on every melee/weapon hit - dispatches to whichever owned
		// augments care about it (most won't override this and do nothing).
		// Normalized boss key — Twins are two NPCs but one fight, so both segments
		// map to Retinazer's type for participation and kill-count tracking.
		private static int BossKey(int npcType)
			=> npcType == NPCID.Spazmatism ? NPCID.Retinazer : npcType;

		// Records that the local player dealt damage to a boss this fight.
		// Sends a packet on the first hit so the server can populate its copy.
		private void TryRegisterBossDamage(NPC target)
		{
			if (!target.boss) return;

			int key = BossKey(target.type);
			if (DamagedBossesThisFight.Contains(key)) return; // already registered

			DamagedBossesThisFight.Add(key);

			if (Main.netMode != NetmodeID.MultiplayerClient) return;

			ModPacket packet = ModContent.GetInstance<Augments>().GetPacket();
			packet.Write((byte)AugmentPacketType.BossDamageParticipation);
			packet.Write((byte)Player.whoAmI);
			packet.Write(key);
			packet.Send();
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			TryRegisterBossDamage(target);

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
			TryRegisterBossDamage(target);

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

			// Pull-based Martyr's Resolve: check nearby Support players for the augment
			// and apply 15% incoming damage reduction to the local player.
			// The Support player's corresponding self-penalty (+15% damage taken) is
			// applied inside MartyrsResolveAugment.ModifyHurt as a normal augment hook.
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player other = Main.player[i];
				if (!other.active || other.dead || other == Player)
					continue;
				if (Vector2.Distance(Player.Center, other.Center) > AuraRadius)
					continue;
				var otherAP = other.GetModPlayer<AugmentPlayer>();
				if (!otherAP.HasAugment("martyrs_resolve"))
					continue;
				modifiers.FinalDamage *= 0.85f;
				break;
			}
		}

		// Lifeline: fires on the dying player's own client the moment HP hits 0.
		// Returning false prevents death; returning true allows it.
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (LifelineCooldown > 0)
				return true;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player other = Main.player[i];
				if (!other.active || other.dead || other == Player)
					continue;
				if (Vector2.Distance(Player.Center, other.Center) > AuraRadius)
					continue;
				var otherAP = other.GetModPlayer<AugmentPlayer>();
				if (!otherAP.HasAugment("lifeline"))
					continue;

				LifelineCooldown = 5400;
				lifelineInvulnTicks = 120;
				Player.statLife = 1;
				Player.AddBuff(ModContent.BuffType<LifelineCooldownBuff>(), 5400);
				return false;
			}

			return true;
		}

		// Undying Bond: fires every tick while the local player is dead.
		// Overrides SpawnX/SpawnY to redirect respawn to the nearest Support player
		// who owns "undying_bond". No range limit — works regardless of distance.
		// MULTIPLAYER FLAG: SpawnX/SpawnY are modified on the dead player's client.
		// If the server uses its own copy for the respawn calculation, this will only
		// work in singleplayer and a ModPacket sync will be needed for multiplayer.
		public override void UpdateDead()
		{
			Player nearest = null;
			float nearestDist = float.MaxValue;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player other = Main.player[i];
				if (!other.active || other.dead || other == Player)
					continue;
				var otherAP = other.GetModPlayer<AugmentPlayer>();
				if (!otherAP.HasAugment("undying_bond"))
					continue;
				float dist = Vector2.Distance(Player.Center, other.Center);
				if (dist < nearestDist)
				{
					nearestDist = dist;
					nearest = other;
				}
			}

			if (nearest != null)
			{
				Player.SpawnX = (int)(nearest.Center.X / 16f);
				Player.SpawnY = (int)(nearest.Center.Y / 16f);
			}
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			foreach (var a in Owned)
				a.OnKill(Player);
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

		private void RebuildOwnedCacheFromOwnedIdsOnly()
		{
			owned.Clear();
			foreach (string id in ownedIds)
			{
				Augment augment = AugmentDatabase.GetById(id);
				if (augment != null)
					owned.Add(augment);
			}

			DebugCheckState("RebuildOwnedCacheFromOwnedIdsOnly");
		}

		public void DebugCheckState(string where)
		{
			foreach (string id in soldAugmentIds)
			{
				if (!ownedIds.Contains(id))
					continue;

				string bug = $"BUG STATE at {where}: {id} is BOTH owned and sold";
				ModContent.GetInstance<Augments>().Logger.Error(bug);
				if (Main.netMode != NetmodeID.Server && !Main.dedServ)
					Main.NewText(bug, 255, 80, 80);
			}

			string state = $"{where} ({Player.name}): Owned=[{string.Join(",", ownedIds)}] Sold=[{string.Join(",", soldAugmentIds)}] Ever=[{string.Join(",", everOwnedIds)}]";
			ModContent.GetInstance<Augments>().Logger.Info(state);
			if (Main.netMode != NetmodeID.Server && !Main.dedServ)
				Main.NewText(state, 180, 180, 180);
		}

		private void SyncInventory()
		{
			if (Main.netMode != NetmodeID.Server)
				return;

			for (int slot = 0; slot < Player.inventory.Length; slot++)
				NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, Player.whoAmI, slot, Player.inventory[slot].prefix);
		}

		public override void Initialize()
		{
			ownedIds.Clear();
			everOwnedIds.Clear();
			soldAugmentIds.Clear();
			lockedKeystoneFamilies.Clear();
			owned.Clear();
			BossAugmentKills = new Dictionary<int, int>();
			DamagedBossesThisFight = new HashSet<int>();
		}

		public override void OnEnterWorld()
		{
			DamagedBossesThisFight.Clear();
			AugmentNet.SendRequestAugmentSync();
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			if (Main.netMode == NetmodeID.Server)
				AugmentNet.SendSyncPlayer(Player, toWho);
		}

		public void WriteAugmentState(BinaryWriter writer)
		{
			DebugCheckState("WriteAugmentState");
			writer.Write((ushort)ownedIds.Count);
			foreach (string id in ownedIds)
				writer.Write(id);

			writer.Write((ushort)everOwnedIds.Count);
			foreach (string id in everOwnedIds)
				writer.Write(id);

			writer.Write((ushort)soldAugmentIds.Count);
			foreach (string id in soldAugmentIds)
				writer.Write(id);

			writer.Write((ushort)lockedKeystoneFamilies.Count);
			foreach (string family in lockedKeystoneFamilies)
				writer.Write(family);
		}

		public void ReadAugmentState(BinaryReader reader)
		{
			ownedIds.Clear();
			ushort ownedCount = reader.ReadUInt16();
			for (int i = 0; i < ownedCount; i++)
				ownedIds.Add(reader.ReadString());

			everOwnedIds.Clear();
			ushort everOwnedCount = reader.ReadUInt16();
			for (int i = 0; i < everOwnedCount; i++)
				everOwnedIds.Add(reader.ReadString());

			soldAugmentIds.Clear();
			ushort soldCount = reader.ReadUInt16();
			for (int i = 0; i < soldCount; i++)
				soldAugmentIds.Add(reader.ReadString());

			lockedKeystoneFamilies.Clear();
			ushort lockedFamilyCount = reader.ReadUInt16();
			for (int i = 0; i < lockedFamilyCount; i++)
				lockedKeystoneFamilies.Add(reader.ReadString());

			RebuildOwnedCacheFromOwnedIdsOnly();
			DebugCheckState("ReadAugmentState");
		}

		// --- Persistence: augments survive between play sessions ---

		public override void SaveData(TagCompound tag)
		{
			DebugCheckState("SaveData");
			var customData = new TagCompound();
			foreach (var a in owned)
			{
				var augmentTag = new TagCompound();
				a.SaveCustomData(augmentTag);
				if (augmentTag.Count > 0)
					customData[a.Id] = augmentTag;
			}
			tag["ownedAugmentIds"] = new List<string>(ownedIds);
			tag["augmentCustomData"] = customData;
			tag["everOwnedIds"] = new List<string>(everOwnedIds);
			tag["soldAugmentIds"] = new List<string>(soldAugmentIds);
			tag["lockedKeystoneFamilies"] = new List<string>(lockedKeystoneFamilies);

			// TagCompound requires string keys — store NPC type as string.
			var bossKills = new TagCompound();
			foreach (var kv in BossAugmentKills)
				bossKills[kv.Key.ToString()] = kv.Value;
			tag["bossAugmentKills"] = bossKills;
		}

		public override void LoadData(TagCompound tag)
		{
			ownedIds.Clear();
			everOwnedIds.Clear();
			soldAugmentIds.Clear();
			lockedKeystoneFamilies.Clear();

			string ownedKey = tag.ContainsKey("ownedAugmentIds") ? "ownedAugmentIds" : "augmentIds";
			if (tag.ContainsKey(ownedKey))
			{
				foreach (string id in tag.GetList<string>(ownedKey))
				{
					if (ownedIds.Count >= MaxOwnedAugments)
						break;
					if (AugmentDatabase.GetById(id) != null)
						ownedIds.Add(id);
				}
			}

			if (tag.ContainsKey("everOwnedIds"))
				everOwnedIds.UnionWith(tag.GetList<string>("everOwnedIds"));
			everOwnedIds.UnionWith(ownedIds);

			if (tag.ContainsKey("soldAugmentIds"))
				soldAugmentIds.UnionWith(tag.GetList<string>("soldAugmentIds"));
			else
			{
				// Legacy saves inferred buyback history as EverOwned minus Owned.
				foreach (string id in everOwnedIds)
				{
					if (!ownedIds.Contains(id))
						soldAugmentIds.Add(id);
				}
			}
			everOwnedIds.UnionWith(soldAugmentIds);

			if (tag.ContainsKey("lockedKeystoneFamilies"))
				lockedKeystoneFamilies.UnionWith(tag.GetList<string>("lockedKeystoneFamilies"));

			RebuildOwnedCacheFromOwnedIdsOnly();

			if (tag.GetCompound("augmentCustomData") is TagCompound customData)
			{
				foreach (var a in owned)
				{
					if (customData.GetCompound(a.Id) is TagCompound augmentTag)
						a.LoadCustomData(augmentTag);
				}
			}

			BossAugmentKills.Clear();
			if (tag.ContainsKey("bossAugmentKills"))
			{
				TagCompound bossKills = tag.GetCompound("bossAugmentKills");
				foreach (var kv in bossKills)
				{
					if (int.TryParse(kv.Key, out int npcType))
						BossAugmentKills[npcType] = (int)kv.Value;
				}
			}

			// Saves predate Keystone tracking too - backfill from whatever
			// Keystone is currently owned so a save made before this feature
			// existed still correctly locks out that Keystone's siblings.
			foreach (var a in owned)
			{
				if (a.KeystoneFamily != null)
					lockedKeystoneFamilies.Add(a.KeystoneFamily);
			}

			DebugCheckState("LoadData");
		}
	}
}
