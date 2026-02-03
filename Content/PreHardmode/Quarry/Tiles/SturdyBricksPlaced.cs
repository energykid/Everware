using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Quarry.Tiles;

public class SturdyBricksPlaced : EverTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        DustType = DustID.Stone;
        HitSound = SoundID.Tink;
        AddMapEntry(new Color(101, 101, 101));
    }
}
