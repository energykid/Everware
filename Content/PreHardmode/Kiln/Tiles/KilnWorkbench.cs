using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class KilnWorkbench : EverPlaceableItem
{
    public override int PlacementID => ModContent.TileType<KilnWorkbenchPlaced>();

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(20);
        recipe.AddIngredient(ModContent.ItemType<KilnBrick>(), 10);
        recipe.AddIngredient(ItemID.Leather, 1);
        recipe.AddTile(TileID.WorkBenches);
        recipe.Register();
    }
}
