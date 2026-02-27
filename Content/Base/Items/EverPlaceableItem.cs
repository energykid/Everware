using Terraria.ID;

namespace Everware.Content.Base.Items;

public abstract class EverPlaceableItem : EverItem
{
    public virtual int PlacementID => TileID.Stone;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToPlaceableTile(PlacementID);
    }
}
