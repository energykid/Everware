using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Meteor.Tiles;

public class CharredSoilWall : ModWall
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/CharredSoilWall";
    public override void SetStaticDefaults()
    {
        DustType = DustID.Asphalt;
        AddMapEntry(new Color(7, 4, 2));
    }
}

public class CharredSoilWallItem : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/CharredSoilWallItem";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<CharredSoilWall>());
        Item.value = Sell.Silver(1);
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe(4);
        recipe.AddIngredient(ModContent.ItemType<CharredSoilItem>(), 1);
        recipe.AddTile(TileID.WorkBenches);
        recipe.Register();

        Recipe recipe1 = Recipe.Create(ModContent.ItemType<CharredSoilItem>(), 1);
        recipe1.AddIngredient(Type, 4);
        recipe1.AddTile(TileID.WorkBenches);
        recipe1.Register();
    }
}
