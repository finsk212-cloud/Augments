using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Augments
{
    // First pass: a recruitable town resident with no shop yet. Moves in the
    // same simple way the Demolitionist does (downed-boss flag + vacant
    // house), not the Goblin Tinkerer "find them in the world" style - that's
    // a planned later refinement.
    public class AugmentVendorNPC : ModNPC
    {
        // TEMPORARY PLACEHOLDER ART: no mod-owned sprite exists yet, so this
        // points at the Stylist's own vanilla texture instead of a file in
        // this mod, for anything PreDraw below doesn't cover (head icon,
        // minimap, bestiary, etc). tModLoader can load a vanilla asset path
        // this way without requiring an asset file under this mod's folders -
        // replace with a real "NPCs/AugmentVendorNPC" png (and drop this
        // override and the recolor logic below) once actual art exists.
        public override string Texture => "Terraria/Images/NPC_" + NPCID.Stylist;

        // Recolored copy of the Stylist's sprite sheet (white hair, black
        // clothing), built once on first draw and cached for the rest of the
        // session. See GetRecoloredTexture for honest caveats about how
        // approximate this color match actually is.
        private static Texture2D recoloredTexture;

        // Anchor colors and tolerances are an informed GUESS (auburn/copper
        // hair, dark dress), not a verified pixel sample - this sandboxed
        // environment has no way to download the live game texture or any
        // image file and inspect its actual raw pixels, only to read text
        // descriptions of it. Treat this as a first-pass approximation that
        // needs a real in-game look before trusting it.
        private static readonly Color HairAnchor = new Color(150, 70, 45);
        private const float HairTolerance = 70f;

        private static readonly Color ClothingAnchor = new Color(25, 20, 25);
        private const float ClothingTolerance = 55f;

        private static Texture2D GetRecoloredTexture()
        {
            if (recoloredTexture != null)
                return recoloredTexture;

            // TextureAssets.Npc[NPCID.Stylist] is only requested with DoNotLoad
            // by vanilla until a real Stylist is on-screen, so .Value would
            // hand back a 1x1 placeholder here - request it directly with
            // ImmediateLoad to force the real pixel data to load synchronously.
            Texture2D source = ModContent.Request<Texture2D>("Terraria/Images/NPC_" + NPCID.Stylist, AssetRequestMode.ImmediateLoad).Value;

            var pixels = new Color[source.Width * source.Height];
            source.GetData(pixels);

            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                if (pixel.A == 0)
                    continue;

                if (ColorDistance(pixel, HairAnchor) <= HairTolerance)
                    pixels[i] = new Color(255, 255, 255, pixel.A);
                else if (ColorDistance(pixel, ClothingAnchor) <= ClothingTolerance)
                    pixels[i] = new Color(0, 0, 0, pixel.A);
            }

            recoloredTexture = new Texture2D(Main.graphics.GraphicsDevice, source.Width, source.Height);
            recoloredTexture.SetData(pixels);
            return recoloredTexture;
        }

        private static float ColorDistance(Color a, Color b)
        {
            float dr = a.R - b.R;
            float dg = a.G - b.G;
            float db = a.B - b.B;
            return (float)System.Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = GetRecoloredTexture();
            int frameHeight = texture.Height / Main.npcFrameCount[NPCID.Stylist];
            // NPC.frame.Y is already the absolute pixel offset of the current
            // frame within the sheet (the vanilla animation code sets it to
            // frameHeight * frameIndex), not a frame index - use it directly
            // instead of multiplying by frameHeight again, or the source rect
            // ends up entirely outside the texture and draws nothing.
            var sourceRect = new Rectangle(0, NPC.frame.Y, texture.Width, frameHeight);
            var origin = new Vector2(sourceRect.Width * 0.5f, sourceRect.Height * 0.5f);

            spriteBatch.Draw(
                texture,
                NPC.Center - screenPos,
                sourceRect,
                drawColor,
                NPC.rotation,
                origin,
                NPC.scale,
                NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0f);

            return false;
        }

        public override void SetStaticDefaults()
        {
            // Frame count/animation are copied from the Stylist to match the
            // recolored texture above - update both together once this NPC
            // has its own sprite sheet.
            Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Stylist];
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = NPCAIStyleID.Passive;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;

            // Reuses the Stylist's standard idle/wander/sleep town behavior
            // wholesale, rather than writing custom movement AI.
            AnimationType = NPCID.Stylist;
        }

        // Only allowed to move into town once Skeletron is downed - same
        // gate the Demolitionist itself doesn't use, but the simplest
        // boss-flag-gated recruitment style requested for this pass.
        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            return NPC.downedBoss3;
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string>
            {
                "Augustus",
                "Ferro",
                "Vex"
            };
        }

        public override string GetChat()
        {
            string[] lines =
            {
                "I've heard whispers of strange augments out there. Wish I knew more.",
                "Don't mind the mess - I'm still settling in.",
                "No wares to show yet. Check back later."
            };

            return lines[Main.rand.Next(lines.Length)];
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "Wares";
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (!firstButton)
                return;

            // Leave shopName untouched so vanilla's own shop UI doesn't also
            // open. Closing the chat window has to be deferred a frame:
            // vanilla's own GUIChatDrawInner calls this hook and then
            // immediately re-indexes npc[player[myPlayer].talkNPC] afterward
            // (to check vanilla NPC types for its own shop dispatch) - if we
            // reset talkNPC synchronously here, that re-index reads npc[-1]
            // and throws IndexOutOfRangeException. QueueMainThreadAction runs
            // this at the start of the next Update, after that vanilla call
            // has already finished.
            Main.QueueMainThreadAction(Main.CloseNPCChatOrSign);
            ModContent.GetInstance<AugmentUISystem>().ShowShop();
        }
    }
}
