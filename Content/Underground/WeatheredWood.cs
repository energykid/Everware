using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Underground;

public class WeatheredWood : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Underground/WeatheredWood";
    public override int PlacementID => ModContent.TileType<WeatheredWoodPlaced>();

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(1);
        recipe.AddIngredient(ItemID.Wood, 1);
        recipe.AddTile(TileID.LivingLoom);
        recipe.Register();
    }
}
