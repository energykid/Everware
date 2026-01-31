using Terraria.Audio;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Everware.Content.PreHardmode.Kiln.Visual;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class KilnBrickWallPlaced : ModWall
{
    public override void SetStaticDefaults()
    {
        DustType = ModContent.DustType<KilnPowderDust>();
        SoundStyle style = new SoundStyle("Everware/Sounds/Tile/KilnstoneHit");
        AddMapEntry(new Color(181, 62, 59));
        style.PitchRange = (0.2f, 0.4f);
        HitSound = style;
    }
}
