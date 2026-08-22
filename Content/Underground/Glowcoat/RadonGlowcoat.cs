using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Linq;
using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class RadonGlowcoat : BaseGlowcoatItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return ModLoader.TryGetMod("SpiritReforged", out Mod reforged);
    }

    public override Color Color => new Color(200, 200, 55);
    public override int DustType => DustID.YellowTorch;
    public override Asset<Texture2D> GlowAsset => Assets.Textures.Underground.RadonGlowcoat_Glow.Asset;
    public override string Texture => "Everware/Assets/Textures/Underground/RadonGlowcoat";

    public override void AddRecipes()
    {
        int moss = TileID.KryptonMossBlock;
        int mossItem = ItemID.KryptonMoss;
        if (ModLoader.TryGetMod("SpiritReforged", out Mod reforged))
        {
            var blocks = reforged.GetContent<ModTile>();

            ModTile? tile = blocks.FirstOrDefault(A => { return A.Name == "RadonMoss"; }, null);
            moss = tile != null ? tile.Type : moss;

            var items = reforged.GetContent<ModItem>();

            ModItem? item = items.FirstOrDefault(A => { return A.Name == "RadonMossItem"; }, null);
            mossItem = item != null ? item.Type : mossItem;

            Recipe recipe = CreateRecipe(5);
            recipe.AddTile(moss);
            recipe.AddIngredient(ItemID.BottledWater);
            recipe.Register();

            Recipe recipe2 = CreateRecipe(10);
            recipe2.AddTile(TileID.DyeVat);
            recipe2.AddIngredient(ItemID.BottledWater);
            recipe2.AddIngredient(mossItem);
            recipe2.Register();
        }
    }
}
