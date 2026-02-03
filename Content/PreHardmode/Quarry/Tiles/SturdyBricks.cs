using Everware.Content.Base.Items;
using Everware.Content.PreHardmode.Kiln.Tiles;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Quarry.Tiles;

internal class SturdyBricks : EverPlaceableItem
{
    public override int PlacementID => ModContent.TileType<SturdyBricksPlaced>();

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(2);
        recipe.AddIngredient(ItemID.GrayBrick, 1);
        recipe.AddIngredient(ItemID.SiltBlock, 1);
        recipe.AddTile(TileID.Furnaces);
        recipe.Register();
    }
}
