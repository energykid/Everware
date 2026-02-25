using Everware.Content.Base.Items;

namespace Everware.Content.Kiln.Tiles;

public class ForgingKilnItem : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Kiln/ForgingKilnItem";
    public override int DuplicationAmount => 1;
    public override int PlacementID => ModContent.TileType<ForgingKiln>();
}
