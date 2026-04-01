using Everware.Content.Base.Items;
using Everware.Content.Gallery;
using Terraria.ID;

namespace Everware.Content;

public class TestItem : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Misc/TestItem";
    public override bool IsLoadingEnabled(Mod mod)
    {
        return true;
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
            GalleryGenerator.GenerateGallery((Main.MouseWorld / 16).ToPoint());
        }

        return base.UseItem(player);
    }
}
