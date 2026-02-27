using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.Quarry.Tiles;

public class SturdyBricksPlaced : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Quarry/SturdyBricksPlaced";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        DustType = DustID.Stone;
        HitSound = SoundID.Tink;
        AddMapEntry(new Color(101, 101, 101));
    }
}
