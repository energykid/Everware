using Everware.Common.Players;

namespace Everware.Content.Base.Tiles.TileData;

public struct LastPlayerMinedData : ITileData
{
    public int WhichPlayerAmI;
}

public class LastPlayerMinedItem : GlobalItem
{
    public override bool? UseItem(Item item, Player player)
    {
        if (base.UseItem(item, player).Equals(true))
        {
            Tile? tile = Main.tile[(player.GetModPlayer<NetworkPlayer>().MousePosition / 16).ToPoint()];

            if (tile.HasValue)
            {
                if (tile.Value.HasTile)
                {
                    if (item.pick > 0)
                    {
                        tile.Value.Get<LastPlayerMinedData>().WhichPlayerAmI = player.whoAmI;
                        return base.UseItem(item, player);
                    }
                }
            }
        }
        return base.UseItem(item, player);
    }
}