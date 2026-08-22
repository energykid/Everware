using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.Underground;

public class WeatheredWoodPlaced : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Underground/WeatheredWoodPlaced";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.tileSolid[Type] = true;
        DustType = DustID.WoodFurniture;
        AddMapEntry(new Color(111, 82, 60));
    }
}
