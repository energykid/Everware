using Terraria.ID;

namespace Everware.Content.Kiln.Tiles;

public class WornFencePlaced : ModWall
{
    public override string Texture => "Everware/Assets/Textures/Kiln/WornFencePlaced";
    public override void SetStaticDefaults()
    {
        DustType = DustID.WoodFurniture;
        AddMapEntry(new Color(66, 52, 40));
        Main.wallHouse[Type] = true;
    }
}
