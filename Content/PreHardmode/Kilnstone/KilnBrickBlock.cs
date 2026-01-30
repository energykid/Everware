using Everware.Content.Base.Tiles;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;

namespace Everware.Content.PreHardmode.Kilnstone;

public class KilnBrickBlock : EverTile 
{
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