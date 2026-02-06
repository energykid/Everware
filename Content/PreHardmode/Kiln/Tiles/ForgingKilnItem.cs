using Everware.Content.Base.Items;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class ForgingKilnItem : EverPlaceableItem
{
    public override int DuplicationAmount => 1;
    public override int PlacementID => ModContent.TileType<ForgingKiln>();
}
