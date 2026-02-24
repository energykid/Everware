using Everware.Content.Base.Items;

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
