using Everware.Content.Base.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Everware.Content.PreHardmode.Kilnstone;

public class Kilnstone : EverPlaceableItem
{
    public override int PlacementID => ModContent.TileType<KilnstoneBlock>();

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(20);
        recipe.AddIngredient(ItemID.ClayBlock, 15);
        recipe.AddIngredient(ItemID.StoneBlock, 5);
        recipe.AddTile(TileID.Furnaces);
        recipe.Register();
    }
}
