using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Kiln.Tiles;

public class Kilnstone : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Kiln/Kilnstone";
    public override int PlacementID => ModContent.TileType<KilnstonePlaced>();

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(2);
        recipe.AddIngredient(ItemID.ClayBlock, 1);
        recipe.AddIngredient(ItemID.StoneBlock, 1);
        recipe.AddTile(ModContent.TileType<ForgingKiln>());
        recipe.Register();

        Recipe recipe2 = CreateRecipe(1);
        recipe2.AddIngredient(ItemID.DirtBlock, 1);
        recipe2.AddIngredient(ItemID.StoneBlock, 1);
        recipe2.AddTile(ModContent.TileType<ForgingKiln>());
        recipe2.Register();
    }
}
