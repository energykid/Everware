using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace Everware.Content.Base.Items;

public abstract class EverPlaceableItem : EverItem
{
    public virtual int PlacementID => TileID.Stone;

    public override int DuplicationAmount => 999;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToPlaceableTile(PlacementID);
    }
}
