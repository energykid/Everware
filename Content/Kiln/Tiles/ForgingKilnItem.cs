using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Kiln.Tiles;

public class ForgingKilnItem : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Kiln/ForgingKilnItem";
    public override int DuplicationAmount => 1;
    public override int PlacementID => ModContent.TileType<ForgingKiln>();
    public override void AddRecipes()
    {
        Recipe rc = Recipe.Create(Type);
        rc.AddTile(TileID.Anvils);
        rc.AddIngredient(ModContent.ItemType<Kilnstone>(), 15);
        rc.AddIngredient(ItemID.Torch, 5);
        rc.AddRecipeGroup(RecipeGroupID.IronBar, 10);
        rc.Register();
    }
}
