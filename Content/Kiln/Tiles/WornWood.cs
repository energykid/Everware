using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Kiln.Tiles;

public class WornWood : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Kiln/WornWood";
    public override int PlacementID => ModContent.TileType<WornWoodPlaced>();

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(2);
        recipe.AddIngredient(ItemID.Wood, 1);
        recipe.AddTile(TileID.Sawmill);
        recipe.Register();
    }
}
