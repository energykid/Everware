using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Kiln.Tiles;

public class KilnstoneWall : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnstoneWall";
    public override int PlacementID => ModContent.WallType<KilnstoneWallPlaced>();
    public override bool Wall => true;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Sell.Copper(5);
    }

    public override void AddRecipes()
    {
        Recipe recipe1 = CreateRecipe(4);
        recipe1.AddIngredient(ModContent.ItemType<Kilnstone>(), 1);
        recipe1.AddTile(TileID.WorkBenches);
        recipe1.Register();

        Recipe recipe2 = Recipe.Create(ModContent.ItemType<Kilnstone>(), 1);
        recipe2.AddIngredient(this, 4);
        recipe2.AddTile(TileID.WorkBenches);
        recipe2.Register();
    }
}
