using System;
using Terraria;

namespace Augments
{
    public class ManaRefluxAugment : Augment
    {
        public override string Id => "mana_reflux";
        public override string DisplayName => "Mana Reflux";
        public override string Description =>
            $"When you take damage, restore {AugmentText.Mana("mana")} equal to {AugmentText.Mana("35% of the damage taken")}.";

        public override AugmentRarity Rarity => AugmentRarity.Common;
        public override AugmentClass Class => AugmentClass.Universal;

        public override void OnHurt(Player player, Player.HurtInfo info)
        {
            int manaRestored = (int)(info.Damage * 0.35f);

            if (manaRestored <= 0)
                return;

            player.statMana = Math.Min(player.statMana + manaRestored, player.statManaMax2);
            player.ManaEffect(manaRestored);
        }
    }
}
