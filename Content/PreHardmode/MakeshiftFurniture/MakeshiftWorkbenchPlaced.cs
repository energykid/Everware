using Everware.Content.Base.Tiles;
using Everware.Content.PreHardmode.Kiln.Visual;

namespace Everware.Content.PreHardmode.MakeshiftFurniture;

public class MakeshiftWorkbenchPlaced : WorkbenchTemplate
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = ModContent.DustType<KilnPowderDust>();
        SoundStyle style = new SoundStyle("Everware/Sounds/Tile/KilnstoneHit");
        AddMapEntry(new Color(181, 62, 59));
        style.PitchRange = (0f, 0.2f);
        HitSound = style;
    }
}
