using Everware.Content.Kiln.Visual;

namespace Everware.Content.Kiln.Tiles;

public class KilnBrickWallPlaced : ModWall
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnBrickWallPlaced";
    public override void SetStaticDefaults()
    {
        DustType = ModContent.DustType<KilnPowderDust>();
        SoundStyle style = new SoundStyle("Everware/Assets/Sounds/Tile/KilnstoneHit");
        AddMapEntry(new Color(181, 62, 59));
        style.PitchRange = (0.2f, 0.4f);
        HitSound = style;
        Main.wallHouse[Type] = true;
    }
}
