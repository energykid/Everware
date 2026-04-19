using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Linq;
using Terraria.ID;

namespace Everware.Content.Underground.Glowcoat;

public class OganessonGlowcoat : BaseGlowcoatItem
{
    public override bool IsLoadingEnabled(Mod mod)
    {
        return ModLoader.TryGetMod("SpiritReforged", out Mod reforged);
    }

    public override Color Color => new Color(185, 185, 185);
    public override int DustType => DustID.WhiteTorch;
    public override Asset<Texture2D> GlowAsset => Assets.Textures.Underground.OganessonGlowcoat_Glow.Asset;
    public override string Texture => "Everware/Assets/Textures/Underground/OganessonGlowcoat";

    public override void AddRecipes()
    {
        int moss = TileID.KryptonMossBlock;
        int mossItem = ItemID.KryptonMoss;
        if (ModLoader.TryGetMod("SpiritReforged", out Mod reforged))
        {
            var blocks = reforged.GetContent<ModTile>();

            ModTile? tile = blocks.FirstOrDefault(A => { return A.Name == "OganessonMoss"; }, null);
            moss = tile != null ? tile.Type : moss;

            var items = reforged.GetContent<ModItem>();

            ModItem? item = items.FirstOrDefault(A => { return A.Name == "OganessonMossItem"; }, null);
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
