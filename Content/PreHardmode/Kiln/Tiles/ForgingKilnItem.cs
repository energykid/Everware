using Everware.Content.Base.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class ForgingKilnItem : EverPlaceableItem
{
    public override int PlacementID => ModContent.TileType<ForgingKiln>();
}
