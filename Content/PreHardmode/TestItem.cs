using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.PreHardmode;

public class TestItem : EverItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return false;
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 20;
    }
    public override bool? UseItem(Player player)
    {
        if (player.ItemAnimationJustStarted)
        {

            SoundEngine.PlaySound(SoundID.Grass);
        }

        return base.UseItem(player);
    }
}
