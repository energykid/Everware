using Terraria.ID;
using Terraria.Localization;

namespace Everware.Content.Base.Items;

public abstract class EverPickupItem : EverItem
{
    public override LocalizedText Tooltip => LocalizedText.Empty;
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatShouldNotBeInInventory[Type] = true;
        ItemID.Sets.IgnoresEncumberingStone[Type] = true;
        ItemID.Sets.IsAPickup[Type] = true;
        ItemID.Sets.ItemSpawnDecaySpeed[Type] = 4;
    }
    public override bool CanStackInWorld(Item source)
    {
        return false;
    }
    public override bool OnPickup(Player player)
    {
        return false;
    }
}
