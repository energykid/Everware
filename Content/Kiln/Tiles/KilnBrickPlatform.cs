using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Kiln.Tiles;

public class KilnBrickPlatform : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnBrickPlatform";
    public override int PlacementID => ModContent.TileType<KilnBrickPlatformPlaced>();

    public override void AddRecipes()
    {
        Recipe recipe1 = CreateRecipe(2);
        recipe1.AddIngredient(ModContent.ItemType<KilnBrick>(), 1);
        recipe1.AddTile(TileID.WorkBenches);
        recipe1.Register();

        Recipe recipe2 = Recipe.Create(ModContent.ItemType<KilnBrick>(), 1);
        recipe2.AddIngredient(this, 2);
        recipe2.AddTile(TileID.WorkBenches);
        recipe2.Register();
    }
}
