using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Augments
{
	public class TrophyHunterAugment : Augment
	{
		public override string Id => "trophy_hunter";
		public override string DisplayName => "Trophy Hunter";
		public override string Description =>
			$"The first melee kill against each unique enemy type permanently grants {AugmentText.BonusDamage("+0.25% melee damage")}. No cap.";

		public override AugmentRarity Rarity => AugmentRarity.Epic;
		public override AugmentClass Class => AugmentClass.Melee;

		private const float BonusPerType = 0.0025f;

		private readonly HashSet<int> killedTypes = new HashSet<int>();

		// Shows the current total bonus percentage in the status row, same
		// StatusValue mechanism VoidStepAugment uses - rounded rather than
		// truncated so e.g. 4 types (1.00%) doesn't read as "0%".
		public override int? StatusValue => killedTypes.Count > 0 ? (int)System.Math.Round(killedTypes.Count * BonusPerType * 100f) : (int?)null;
		public override string StatusValueSuffix => "%";

		public override void UpdateEquips(Player player)
		{
			if (killedTypes.Count > 0)
				player.GetDamage(DamageClass.Melee) += killedTypes.Count * BonusPerType;
		}

		public override void OnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit)
		{
			if (item.DamageType == DamageClass.Melee)
				target.GetGlobalNPC<AugmentTrophyHunterNPC>().TagMeleeHit(player.whoAmI);
		}

		public override void OnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit)
		{
			if (proj.DamageType == DamageClass.Melee)
				target.GetGlobalNPC<AugmentTrophyHunterNPC>().TagMeleeHit(player.whoAmI);
		}

		// Same DoT-safe kill credit as Vampiric Edge: a kill finished off by a
		// melee-inflicted DoT (e.g. Bloodletter's bleed ticking the target
		// down after the original melee hit applied it) still counts here,
		// since OnKillNPC alone can't tell what damage type landed the
		// killing blow - only the tag left by an actual melee hit can.
		public override void OnKillNPC(Player player, NPC npc)
		{
			if (!IsHostileEnemy(npc))
				return;

			var marker = npc.GetGlobalNPC<AugmentTrophyHunterNPC>();
			if (!marker.IsTaggedBy(player.whoAmI))
				return;

			marker.ClearTag();

			if (killedTypes.Add(npc.type))
				SoundEngine.PlaySound(SoundID.AchievementComplete, player.Center);
		}

		// Mirrors vanilla's own NPC.CanBeChasedBy critter/ally filter
		// (lifeMax > 5, not friendly) - the same threshold vanilla itself
		// uses to tell real enemies apart from critters, so town NPCs and
		// critters (vanilla or modded, as long as they follow that same
		// lifeMax <= 5 convention) don't hand out free stacks.
		private static bool IsHostileEnemy(NPC npc) => !npc.friendly && npc.lifeMax > 5;

		public override void SaveCustomData(TagCompound tag)
		{
			tag["killedTypes"] = new List<int>(killedTypes);
		}

		public override void LoadCustomData(TagCompound tag)
		{
			killedTypes.Clear();
			if (tag.ContainsKey("killedTypes"))
				killedTypes.UnionWith(tag.GetList<int>("killedTypes"));
		}
	}
}
