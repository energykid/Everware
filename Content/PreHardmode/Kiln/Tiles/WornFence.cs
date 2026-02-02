using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Kiln.Tiles;

public class WornFence : EverPlaceableItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<WornFencePlaced>());
    }

    public override void AddRecipes()
    {
        Recipe recipe1 = CreateRecipe(4);
        recipe1.AddIngredient(ModContent.ItemType<WornWood>(), 1);
        recipe1.AddTile(TileID.WorkBenches);
        recipe1.Register();

        Recipe recipe2 = Recipe.Create(ModContent.ItemType<WornWood>(), 1);
        recipe2.AddIngredient(this, 4);
        recipe2.AddTile(TileID.WorkBenches);
        recipe2.Register();
    }
}
