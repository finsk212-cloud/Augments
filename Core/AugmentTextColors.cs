using Microsoft.Xna.Framework;

namespace Augments
{
    public static class AugmentTextColors
    {
        public const string TriggerHex = "FFA64D";       // on hit, on kill, trigger text
        public const string HealingHex = "66FF66";       // heal, healing, HP
        public const string CritHex = "FF4D4D";          // crit, crits, critical, critical chance
        public const string BonusDamageHex = "FF6666";   // bonus damage text
        public const string IchorHex = "FFD966";         // Ichor
        public const string ManaHex = "66B2FF";          // mana
        public const string CooldownHex = "BFBFBF";      // cooldown
        public const string DurationHex = "99DDFF";      // 2s, 3s, 5s, seconds
        public const string SpecialDamageHex = "3399FF"; // special damage popups / 4th hit damage
        public const string ImmobilizeHex = "CC99FF";    // debuff names that reduce enemy mobility (Slow, and any future similar effects)
        public const string FrostburnHex = "66FFE0";      // the Frostburn debuff name specifically
        public const string MovementSpeedHex = "CCFF66"; // movement speed percentage text

        public static readonly Color Trigger = new Color(255, 166, 77);
        public static readonly Color Healing = new Color(102, 255, 102);
        public static readonly Color Crit = new Color(255, 77, 77);
        public static readonly Color BonusDamage = new Color(255, 102, 102);
        public static readonly Color Ichor = new Color(255, 217, 102);
        public static readonly Color Mana = new Color(102, 178, 255);
        public static readonly Color Cooldown = new Color(191, 191, 191);
        public static readonly Color Duration = new Color(153, 221, 255);
        public static readonly Color SpecialDamage = new Color(51, 153, 255);
        public static readonly Color Immobilize = new Color(204, 153, 255);
        public static readonly Color Frostburn = new Color(102, 255, 224);
        public static readonly Color MovementSpeed = new Color(204, 255, 102);
    }
}
