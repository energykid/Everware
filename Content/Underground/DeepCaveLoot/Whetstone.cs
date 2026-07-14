using Everware.Content.Base.Items;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Underground.DeepCaveLoot;

public class Whetstone : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/Whetstone";

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.createTile = -1;
        Item.maxStack = 1;
        Item.consumable = true;
    }

    public override bool ConsumeItem(Player player)
    {
        Point p = (player.NetworkHandler().MousePosition / 16).ToPoint();
        if (Main.tile[p].TileType == TileID.SharpeningStation)
        {
            while (Main.tile[p].TileFrameX > 0) p.X--;
            while (Main.tile[p].TileFrameY > 0) p.Y--;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Main.tile[p.X + i, p.Y + j].ResetToType(TileID.Dirt);
                }
            }

            SoundEngine.PlaySound(SoundID.Tink);
        }
        return true;
    }

    public override bool CanUseItem(Player player)
    {
        Point p = (player.NetworkHandler().MousePosition / 16).ToPoint();
        if (Main.tile[p].TileType == TileID.SharpeningStation)
        {
            return true;
        }
        else return false;
    }
}
