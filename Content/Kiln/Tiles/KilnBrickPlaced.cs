using Everware.Content.Base.Tiles;
using Everware.Content.Kiln.Visual;

namespace Everware.Content.Kiln.Tiles;

public class KilnBrickPlaced : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnBrickPlaced";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = ModContent.DustType<KilnPowderDust>();
        SoundStyle style = new SoundStyle("Everware/Sounds/Tile/KilnstoneHit");
        AddMapEntry(new Color(181, 62, 59));
        style.PitchRange = (0.2f, 0.4f);
        HitSound = style;
    }
}