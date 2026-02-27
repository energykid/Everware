using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Quarry.Tiles;

public class RebarRod : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Quarry/RebarRod";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<RebarRodPlaced>());
    }

    public override void AddRecipes()
    {
        Recipe recipe1 = CreateRecipe(8);
        recipe1.AddRecipeGroup(RecipeGroupID.IronBar, 1);
        recipe1.AddTile(ModContent.TileType<WeldingStation>()); // replace with Welding Station ASAP
        recipe1.Register();
    }
}
