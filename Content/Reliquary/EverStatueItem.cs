using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Reliquary;

public abstract class EverStatueItem : EverPlaceableItem
{
    public virtual int BaseStatue => ItemID.AngelStatue;
    public virtual int UpgradeMaterial => ItemID.DirtBlock;
    public virtual int UpgradeStack => 1;
    public override void SetStaticDefaults()
    {
        ChiselablesList.AllChiselables.Add(new(BaseStatue, Type, UpgradeMaterial, UpgradeStack));
    }
    public override int DuplicationAmount => 1;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Sell.Gold(1) + Sell.Silver(50);
    }
}
