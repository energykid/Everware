using Everware.Content.Base.Items;
using Everware.Content.Misc.Tiles;
using Terraria.ID;

namespace Everware.Content.Quarry.Tiles;

internal class SturdyBricks : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Quarry/SturdyBricks";
    public override int PlacementID => ModContent.TileType<SturdyBricksPlaced>();

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(4);
        recipe.AddIngredient(ItemID.GrayBrick, 1);
        recipe.AddIngredient(ItemID.SiltBlock, 1);
        recipe.AddTile(TileID.Furnaces);
        recipe.Register();

        Recipe recipe2 = CreateRecipe(2);
        recipe2.AddIngredient(ItemID.GrayBrick, 1);
        recipe2.AddIngredient(ModContent.ItemType<GravelItem>(), 1);
        recipe2.AddTile(TileID.Furnaces);
        recipe2.Register();
    }
}
