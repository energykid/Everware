using Everware.Content.Base.Tiles;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;

namespace Everware.Content.PreHardmode.Kilnstone;

public class KilnstoneBlock : EverTile 
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = ModContent.DustType<RawKilnPowderDust>();
        SoundStyle style = new SoundStyle("Everware/Sounds/Tile/KilnstoneHit");
        AddMapEntry(new Color(151, 62, 59));
        style.PitchRange = (-0.1f, 0.1f);
        HitSound = style;
    }
}