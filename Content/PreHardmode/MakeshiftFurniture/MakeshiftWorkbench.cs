using Everware.Content.Base.Items;

namespace Everware.Content.PreHardmode.MakeshiftFurniture;

public class MakeshiftWorkbench : EverPlaceableItem
{
    public override int PlacementID => ModContent.TileType<MakeshiftWorkbenchPlaced>();
}
