using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class Kilnstone : EverPlaceableItem
{
    public override int PlacementID => ModContent.TileType<KilnstonePlaced>();

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(2);
        recipe.AddIngredient(ItemID.ClayBlock, 1);
        recipe.AddIngredient(ItemID.StoneBlock, 1);
        recipe.AddTile(ModContent.TileType<ForgingKiln>());
        recipe.Register();
    }
}
