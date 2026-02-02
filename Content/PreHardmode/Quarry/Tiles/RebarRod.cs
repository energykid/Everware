using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Quarry.Tiles;

public class RebarRod : EverPlaceableItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<RebarRodPlaced>());
    }

    public override void AddRecipes()
    {
        Recipe recipe1 = CreateRecipe(6);
        recipe1.AddIngredient(RecipeGroupID.IronBar, 1);
        recipe1.AddTile(TileID.Autohammer); // replace with Welding Station ASAP
        recipe1.Register();
    }
}
