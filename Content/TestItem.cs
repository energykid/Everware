using Everware.Content.Base.Items;
using Everware.Content.Underground.Glowcoat;
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
            SoundEngine.PlaySound(SoundID.Grass);

            Main.tile[(Main.MouseWorld / 16).ToPoint()].Get<GlowcoatTileData>().color = Main.rand.NextBool() ? new Color(228, 140, 80) : new Color(151, 105, 151);
        }

        return base.UseItem(player);
    }
}
