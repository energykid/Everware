using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Reliquary;

public abstract class EverStatueItem : EverPlaceableItem
{
    public virtual int BaseStatue => ItemID.AngelStatue;
    public override void SetStaticDefaults()
    {
        ChiselablesList.AllChiselables.Add(new(BaseStatue, Type));
    }
    public override int DuplicationAmount => 1;
    public override void SetDefaults()
    {
        base.SetDefaults();
    }
}
