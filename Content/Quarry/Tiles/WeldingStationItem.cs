using Everware.Content.Base.Items;

namespace Everware.Content.Quarry.Tiles;

public class WeldingStationItem : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Quarry/WeldingStationItem";
    public override int DuplicationAmount => 1;
    public override int PlacementID => ModContent.TileType<WeldingStation>();
}
