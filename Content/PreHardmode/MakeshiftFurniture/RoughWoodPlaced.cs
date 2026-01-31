using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.PreHardmode.MakeshiftFurniture;

public class RoughWoodPlaced : EverTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        DustType = DustID.WoodFurniture;
        AddMapEntry(new Color(111, 82, 60));
    }
}
