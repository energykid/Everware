using Everware.Content.Base.Tiles;
using Everware.Content.Kiln.Visual;

namespace Everware.Content.Kiln.Tiles;

public class KilnWorkbenchPlaced : WorkbenchTemplate
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnWorkbenchPlaced";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = ModContent.DustType<KilnPowderDust>();
        SoundStyle style = new SoundStyle("Everware/Assets/Sounds/Tile/KilnstoneHit");
        AddMapEntry(new Color(181, 62, 59));
        style.PitchRange = (0f, 0.2f);
        HitSound = style;
    }
}
