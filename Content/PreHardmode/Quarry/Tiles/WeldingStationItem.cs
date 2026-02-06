using Everware.Content.Base.Items;

namespace Everware.Content.PreHardmode.Quarry.Tiles;

public class WeldingStationItem : EverPlaceableItem
{
    public override int DuplicationAmount => 1;
    public override int PlacementID => ModContent.TileType<WeldingStation>();
}
