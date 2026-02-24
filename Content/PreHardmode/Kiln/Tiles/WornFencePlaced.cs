using Terraria.ID;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class WornFencePlaced : ModWall
{
    public override void SetStaticDefaults()
    {
        DustType = DustID.WoodFurniture;
        AddMapEntry(new Color(66, 52, 40));
        Main.wallHouse[Type] = true;
    }
}
