using Everware.Content.Base.Items;
using Everware.Content.PreHardmode.Kiln;
using Microsoft.Xna.Framework;
using Terraria;

namespace Everware.Content.PreHardmode;

public class TestItem : EverItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToPlaceableTile(0);
        Item.createTile = -1;
    }
    public override bool? UseItem(Player player)
    {
        if (player.itemAnimation == player.itemAnimationMax - 2)
        {
            Point p = (Main.MouseWorld / 16f).ToPoint();
            for (int i = 0; i < 20; i++) if (!Main.tileSolid[Main.tile[p].TileType] || !Main.tile[p].HasTile) p.Y++;
            KilnGenerator.GenerateKiln(p);
        }
        return base.UseItem(player);
    }
}
