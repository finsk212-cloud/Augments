using Terraria;

namespace Augments
{
    public class ReforgersPatienceAugment : Augment
    {
        public override string Id => "reforgers_patience";
        public override string DisplayName => "Reforger's Patience";
        public override string Description =>
            $"Reforging costs {AugmentText.Cooldown("20% less")}. You can undo your most " +
            "recent reforge once, refunding the cost and restoring the previous prefix.";

        public override AugmentRarity Rarity => AugmentRarity.Epic;
        public override AugmentClass Class => AugmentClass.Universal;

        private const float DiscountMultiplier = 0.8f;

        public override bool ReforgePrice(Player player, Item item, ref int reforgePrice, ref bool canApplyDiscount)
        {
            int discounted = (int)(reforgePrice * DiscountMultiplier);
            reforgePrice = discounted;

            var ap = player.GetModPlayer<AugmentPlayer>();

            // reforgePrice at this point is NOT the final amount BuyItem
            // charges - after every ReforgePrice hook returns, vanilla's own
            // DrawInventory still (a) applies another *0.8 NPC-happiness
            // discount on top if player.discountAvailable, (b) multiplies by
            // player.currentShoppingSettings.PriceAdjustment, and (c) divides
            // by 3 (reforging's standing 1/3-of-sell-price reduction) - none
            // of which any hook can observe directly. Recompute that same
            // chain here so the stashed cost matches what's actually
            // deducted; storing the pre-chain value was refunding far more
            // than the player actually paid.
            long finalPrice = discounted;
            if (canApplyDiscount && player.discountAvailable)
                finalPrice = (long)(finalPrice * 0.8);
            finalPrice = (long)(finalPrice * player.currentShoppingSettings.PriceAdjustment);
            finalPrice /= 3;

            // Stashed here and picked up moments later by PreReforge, within
            // the same synchronous reforge call.
            ap.LastReforgePendingCost = (int)finalPrice;

            return true;
        }

        public override void PreReforge(Player player, Item item)
        {
            var ap = player.GetModPlayer<AugmentPlayer>();

            // item.prefix is still the OLD prefix at this point - vanilla's
            // ResetPrefix()/Prefix() reroll happens right after this hook.
            ap.LastReforgedItem = item;
            ap.LastReforgePrefix = item.prefix;
            ap.LastReforgeCost = ap.LastReforgePendingCost;
        }
    }
}
