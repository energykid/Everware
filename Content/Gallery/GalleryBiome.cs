using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Everware.Content.Gallery;

public class GallerySystem : ModSystem
{
    public static Point GalleryPosition = Point.Zero;
    public override void SaveWorldData(TagCompound tag)
    {
        tag.Set("GalleryPosition", GalleryPosition);
    }
    public override void LoadWorldData(TagCompound tag)
    {
        GalleryPosition = tag.Get<Point>("GalleryPosition");
    }
    public override void PostUpdateEverything()
    {
        if (Main.LocalPlayer.Distance(GallerySystem.GalleryPosition.ToVector2() * 16) < 3000)
        {
            for (int i = -75; i <= 75; i++)
            {
                for (int j = -75; j <= 0; j++)
                {
                    int xx = GallerySystem.GalleryPosition.X + i;
                    int yy = GallerySystem.GalleryPosition.Y + j;
                    if (xx > 250 && xx < Main.maxTilesX - 250)
                    {
                        if (yy > 250 && yy < Main.maxTilesY - 250)
                        {
                            Tile t = Main.tile[xx, yy];
                            if (!t.HasTile && new Vector2(GallerySystem.GalleryPosition.X + i, GallerySystem.GalleryPosition.Y + j).Distance(GallerySystem.GalleryPosition.ToVector2()) < 75)
                                Lighting.AddLight(GallerySystem.GalleryPosition.X + i, GallerySystem.GalleryPosition.Y + j, 0.5f, 0.5f, 1f);
                        }
                    }
                }
            }
        }
    }
}
public class GalleryBiome : ModBiome
{
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override void Load()
    {
        On_Main.DoDraw_WallsAndBlacks += On_Main_DoDraw_WallsAndBlacks;
    }

    private void On_Main_DoDraw_WallsAndBlacks(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self)
    {
        if (!Main.gameMenu)
        {
            Vector2 ORIGIN = new Vector2(1270, 1210);
            Asset<Texture2D> BGMask = Assets.Textures.Gallery.GalleryBackground_Mask.Asset;
            Asset<Texture2D> BGBack = Assets.Textures.Gallery.GalleryBackground_Back.Asset;
            Asset<Texture2D> BGFront = Assets.Textures.Gallery.GalleryBackground_Front.Asset;

            var ParallaxEffect = Assets.Effects.Gallery.GalleryBackgroundEffect.CreateEffect();
            ParallaxEffect.Parameters.MultColor = new Color(0.25f, 0.25f, 0.5f).ToVector4();
            ParallaxEffect.Parameters.TextResolution = BGMask.Size();
            ParallaxEffect.Parameters.FillResolution = BGBack.Size();
            ParallaxEffect.Parameters.Parallax = (Main.LocalPlayer.position.X / -20000f) * Main.caveParallax;
            ParallaxEffect.Parameters.FillTexture = BGBack.Value;
            ParallaxEffect.Apply();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, ParallaxEffect.Shader, Main.BackgroundViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(BGMask.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0.25f, 0.25f, 0.5f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.BackgroundViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(BGFront.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0.25f, 0.25f, 0.5f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);
        }
        orig(self);
    }

    public override void Unload()
    {
        On_Main.DoDraw_WallsAndBlacks -= On_Main_DoDraw_WallsAndBlacks;
    }

    public override string Name => "TheGallery";
    public override int Music => MusicID.OtherworldlyIce;
    public override bool IsBiomeActive(Player player)
    {
        return player.Distance(GallerySystem.GalleryPosition.ToVector2() * 16) < 2300;
    }
}
