using Terraria.ID;

namespace Everware.Content.Base.Items;

public abstract class EverPlaceableItem : EverItem
{
    public virtual int PlacementID => TileID.Stone;
    public virtual bool Wall => false;

    public override void SetDefaults()
    {
        base.SetDefaults();
        if (!Wall)
            Item.DefaultToPlaceableTile(PlacementID);
        else 
            Item.DefaultToPlaceableWall(PlacementID);
    }
}
