using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class WornWoodPlaced : EverTile
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        DustType = DustID.WoodFurniture;
        AddMapEntry(new Color(111, 82, 60));
    }
}
