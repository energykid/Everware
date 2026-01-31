using Everware.Content.Base.Tiles;
using Everware.Content.PreHardmode.Kiln.Visual;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class KilnstonePlaced : EverTile
{
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