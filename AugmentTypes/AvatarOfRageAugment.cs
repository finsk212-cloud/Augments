using Terraria;

namespace Augments
{
    public class AvatarOfRageAugment : Augment
    {
        public override string Id => "avatar_of_rage";
        public override string DisplayName => "Avatar of Rage";
        public override string Description =>
            $"{AugmentText.BonusDamage("Damage")} scales smoothly up to {AugmentText.BonusDamage("+60%")} the lower your " +
            $"{AugmentText.HP("HP")} is, but you can never be {AugmentText.Healing("healed")} above {AugmentText.HP("50% HP")} " +
            "again, permanently, once chosen.";

        public override AugmentRarity Rarity => AugmentRarity.Epic;
        public override AugmentClass Class => AugmentClass.Universal;

        public override string KeystoneFamily => "path_of_the_berserker";

        private const float MaxBonusDamagePercent = 0.6f;
        private const float HealCapPercent = 0.5f;

        // Not restricted by DamageClass, same as Dark Omen/Guardian's Wrath -
        // this scales every kind of damage alike, melee/ranged/magic/summon.
        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FlatBonusDamage += (int)(item.damage * (1f - player.statLife / (float)player.statLifeMax2) * MaxBonusDamagePercent);
        }

        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FlatBonusDamage += (int)(proj.damage * (1f - player.statLife / (float)player.statLifeMax2) * MaxBonusDamagePercent);
        }

        private int lastLife = -1;

        // Same tick-over-tick statLife comparison VitalEchoAugment uses to
        // catch ANY heal regardless of source - here, instead of reacting to
        // the increase, any increase that would cross the 50% cap gets
        // clamped back down to exactly the cap.
        public override void OnUpdate(Player player)
        {
            int healCap = (int)(player.statLifeMax2 * HealCapPercent);

            if (lastLife != -1 && player.statLife > lastLife && player.statLife > healCap)
                player.statLife = healCap;

            lastLife = player.statLife;
        }
    }
}
