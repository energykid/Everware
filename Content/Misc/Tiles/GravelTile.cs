using Everware.Content.Base.Items;
using Everware.Content.Base.Tiles;
using Terraria.ID;

namespace Everware.Content.Misc.Tiles;

public class GravelTile : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Misc/Tiles/GravelTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.Silt;
        AddMapEntry(new Color(73, 73, 73));
    }
}
public class GravelItem : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Misc/Tiles/GravelItem";
    public override int PlacementID => ModContent.TileType<GravelTile>();
}
