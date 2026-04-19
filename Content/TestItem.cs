using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content;

public class TestItem : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Misc/TestItem";
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
            //player.MountedCenter = pos.ToVector2() * 16f;
        }

        return base.UseItem(player);
    }
}
