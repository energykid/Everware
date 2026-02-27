using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.Kiln.Tiles;

public class WornWoodPlaced : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Kiln/WornWoodPlaced";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        DustType = DustID.WoodFurniture;
        AddMapEntry(new Color(111, 82, 60));
    }
}
