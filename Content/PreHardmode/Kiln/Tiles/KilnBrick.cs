using Everware.Content.Base.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class KilnBrick : EverPlaceableItem
{
    public override int PlacementID => ModContent.TileType<KilnBrickPlaced>();

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(1);
        recipe.AddIngredient(ModContent.ItemType<Kilnstone>(), 2);
        recipe.AddTile(ModContent.TileType<ForgingKiln>());
        recipe.Register();
    }
}
