using Everware.Content.Kiln.Visual;

namespace Everware.Content.Kiln.Tiles;

public class KilnstoneWallPlaced : ModWall
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnstoneWallPlaced";
    public override void SetStaticDefaults()
    {
        DustType = ModContent.DustType<RawKilnPowderDust>();
        SoundStyle style = Assets.Sounds.Tile.KilnstoneHit.Asset;
        AddMapEntry(new Color(181, 62, 59));
        style.PitchRange = (-0.1f, 0.1f);
        HitSound = style;
        Main.wallHouse[Type] = true;
    }
}
