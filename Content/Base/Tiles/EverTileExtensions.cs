using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Everware.Content.Base.Tiles;

public static class EverTileExtensions
{
    public static void DefaultToWorkbench(this ModTile tile)
    {
        tile.AdjTiles = [TileID.WorkBenches];
    }
}
