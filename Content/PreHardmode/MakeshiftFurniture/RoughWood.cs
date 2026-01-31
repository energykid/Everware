using Everware.Content.Base.Items;

namespace Everware.Content.PreHardmode.MakeshiftFurniture;

public class RoughWood : EverPlaceableItem
{
    public override int PlacementID => ModContent.TileType<RoughWoodPlaced>();
}
