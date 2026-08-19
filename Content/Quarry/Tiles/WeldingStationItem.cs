using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Quarry.Tiles;

public class WeldingStationItem : EverPlaceableItem
{
    public override string Texture => "Everware/Assets/Textures/Quarry/WeldingStationItem";
    public override int DuplicationAmount => 1;
    public override int PlacementID => ModContent.TileType<WeldingStation>();

    public override void AddRecipes()
    {
        RecipeGroup rg = new RecipeGroup(() => Mods.Everware.Items.AnyCopperBar.GetTextValue(), ItemID.CopperBar, ItemID.TinBar);

        Recipe rc = Recipe.Create(Type);
        rc.AddTile(TileID.Anvils);
        rc.AddIngredient(ModContent.ItemType<RebarRod>(), 15);
        rc.AddIngredient(rg.RegisteredId, 5);
        rc.AddIngredient(RecipeGroupID.IronBar, 10);
        rc.Register();
    }
}
