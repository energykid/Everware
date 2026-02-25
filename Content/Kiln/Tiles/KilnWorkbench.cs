using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Kiln.Tiles;

public class KilnWorkbench : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnWorkbench";
    public override int PlacementID => ModContent.TileType<KilnWorkbenchPlaced>();

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<KilnBrick>(), 10);
        recipe.AddIngredient(ItemID.Leather, 1);
        recipe.Register();
    }
}
