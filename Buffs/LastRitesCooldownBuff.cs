using Terraria;
using Terraria.ModLoader;

namespace Augments
{
    // Display-only buff shown on the protected player while Last Rites is on cooldown.
    // No gameplay effect — the actual cooldown is tracked in AugmentPlayer.LastRitesCooldown.
    // Texture: placeholder — flag for art pass.
    public class LastRitesCooldownBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_2";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
}
