using Everware.Content.Base.Tiles;
using Everware.Content.Kiln.Visual;

namespace Everware.Content.Kiln.Tiles;

public class KilnstonePlaced : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnstonePlaced";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        SoundStyle style = new SoundStyle("Everware/Sounds/Tile/KilnstoneHit");
        DustType = ModContent.DustType<RawKilnPowderDust>();
        AddMapEntry(new Color(151, 62, 59));
        style.PitchRange = (-0.1f, 0.1f);
        HitSound = style;
    }
}