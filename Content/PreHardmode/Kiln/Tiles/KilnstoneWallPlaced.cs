using Everware.Content.PreHardmode.Kiln.Visual;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class KilnstoneWallPlaced : ModWall
{
    public override void SetStaticDefaults()
    {
        DustType = ModContent.DustType<RawKilnPowderDust>();
        SoundStyle style = new SoundStyle("Everware/Sounds/Tile/KilnstoneHit");
        AddMapEntry(new Color(181, 62, 59));
        style.PitchRange = (-0.1f, 0.1f);
        HitSound = style;
        Main.wallHouse[Type] = true;
    }
}
