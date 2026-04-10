using Everware.Content.Base;
using Terraria.Graphics.Light;
using Terraria.ModLoader.IO;

namespace Everware.Content.Gallery;

public class GallerySystem : ModSystem
{
    public override void Load()
    {
        On_TileLightScanner.GetTileLight += On_TileLightScanner_GetTileLight; ;
    }

    private void On_TileLightScanner_GetTileLight(On_TileLightScanner.orig_GetTileLight orig, TileLightScanner self, int x, int y, out Vector3 outputColor)
    {
        orig(self, x, y, out outputColor);

        if (Main.LocalPlayer.Distance(GalleryPosition.ToVector2() * 16) < 3000)
        {
            Tile t = Main.tile[x, y];
            if (new Vector2(x, y).Distance(GalleryPosition.ToVector2()) < 75 && y < GalleryPosition.Y)
            {
                if (!t.HasTile)
                    outputColor += new Vector3(0.45f, 0.45f, 1f);
            }
        }
    }

    public override void Unload()
    {
        On_TileLightScanner.GetTileLight -= On_TileLightScanner_GetTileLight;
    }
    public static Point GalleryPosition = Point.Zero;
    public override void SaveWorldData(TagCompound tag)
    {
        tag.Set("GalleryPosition", GalleryPosition);
    }
    public override void LoadWorldData(TagCompound tag)
    {
        GalleryPosition = tag.Get<Point>("GalleryPosition");
    }
}
public class GalleryBiome : ModBiome
{
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override void Load()
    {
        On_Main.DoDraw_WallsAndBlacks += DoDraw_WallsAndBlacks;
    }

    private void DoDraw_WallsAndBlacks(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self)
    {
        if (!Main.gameMenu)
        {
            float parallax = (Main.screenPosition.X / -20000f) * Main.caveParallax;
            float parallax2 = ((Main.screenPosition.X - (GallerySystem.GalleryPosition.ToVector2() * 16).X) / -20000f) * Main.caveParallax;
            float parallaxOrigin = ((Main.screenPosition.X - (GallerySystem.GalleryPosition.ToVector2() * 16).X) / 20f) * Main.caveParallax;

            Vector2 ORIGIN = new Vector2(1270, 1210);
            Asset<Texture2D> BGMask = Assets.Textures.Gallery.GalleryBackground_Mask.Asset;
            Asset<Texture2D> BGBack = Assets.Textures.Gallery.GalleryBackground_Back.Asset;
            Asset<Texture2D> BGFront = Assets.Textures.Gallery.GalleryBackground_Front.Asset;
            Asset<Texture2D> BGFrontGlow = Assets.Textures.Gallery.GalleryBackground_Front_Glow.Asset;
            Asset<Texture2D> BGMiddle = Assets.Textures.Gallery.GalleryBackground_Middle.Asset;
            Asset<Texture2D> BGMiddleGlow = Assets.Textures.Gallery.GalleryBackground_Middle_Glow.Asset;

            var ParallaxEffect = Assets.Effects.Gallery.GalleryBackgroundEffect.CreateEffect();
            ParallaxEffect.Parameters.MultColor = new Color(0.25f, 0.25f, 0.5f).ToVector4();
            ParallaxEffect.Parameters.TextResolution = BGMask.Size();
            ParallaxEffect.Parameters.FillResolution = BGBack.Size();
            ParallaxEffect.Parameters.Parallax = parallax;
            ParallaxEffect.Parameters.FillTexture = BGBack.Value;
            ParallaxEffect.Apply();

            var ShineEffect = Assets.Effects.Gallery.GalleryBackgroundGlow.CreateEffect();
            ShineEffect.Parameters.BandWidth = 3f;
            ShineEffect.Parameters.Timer = GlobalTimer.Value / 250f;
            ShineEffect.Parameters.Parallax = parallax2 / 2f;
            ShineEffect.Parameters.FillTexture = BGMask.Value;
            ShineEffect.Apply();

            var GalleryClipColor = Assets.Effects.Gallery.GalleryClip.CreateEffect();
            GalleryClipColor.Parameters.MultColor = new Color(0.5f, 0.5f, 1f).ToVector4();
            GalleryClipColor.Parameters.FillTexture = BGMask.Value;
            GalleryClipColor.Parameters.Parallax = parallax2 / 2f;
            GalleryClipColor.Apply();

            Vector2 midOffset = new Vector2(parallaxOrigin, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, ParallaxEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

            // Looping background texture in the very back
            Main.spriteBatch.Draw(BGMask.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0.25f, 0.25f, 0.5f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, GalleryClipColor.Shader, Main.GameViewMatrix.ZoomMatrix);

            // Middle background
            Main.spriteBatch.Draw(BGMiddle.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0.5f, 0.5f, 1f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, GalleryClipColor.Shader, Main.GameViewMatrix.ZoomMatrix);

            GalleryClipColor.Parameters.MultColor = new Color(0f, 0f, 0f, 0.2f).ToVector4();
            GalleryClipColor.Apply();

            // Middle background etchings (shadows)
            Main.spriteBatch.Draw(BGMiddleGlow.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0f, 0f, 0f, 0.2f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, null, null, ShineEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

            // Middle background etchings (glows)
            Main.spriteBatch.Draw(BGMiddleGlow.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), Color.White, 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);

            // Front background etchings (shadows)
            Main.spriteBatch.Draw(BGFront.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0.5f, 0.5f, 1f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            // Front background etchings (shadows)
            Main.spriteBatch.Draw(BGFrontGlow.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0f, 0f, 0f, 0.2f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            ShineEffect.Parameters.Parallax = 0f;
            ShineEffect.Apply();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, null, null, ShineEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

            // Front background etchings (glows)
            Main.spriteBatch.Draw(BGFrontGlow.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), Color.White, 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main._multiplyBlendState, Main.DefaultSamplerState, null, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

            DrawBlack();
        }
        orig(self);
    }

    public void DrawBlack()
    {
        for (int i = -85; i <= 85; i += 1)
        {
            for (int j = -85; j <= 0; j += 1)
            {
                int xx = GallerySystem.GalleryPosition.X + i;
                int yy = GallerySystem.GalleryPosition.Y + j;
                if (xx > 250 && xx < Main.maxTilesX - 250)
                {
                    if (yy > 250 && yy < Main.maxTilesY - 250)
                    {
                        Tile t = Main.tile[xx, yy];
                        if (new Vector2(GallerySystem.GalleryPosition.X + i, GallerySystem.GalleryPosition.Y + j).Distance(GallerySystem.GalleryPosition.ToVector2()) < 85)
                        {
                            if (t.HasTile && Lighting.GetColor(xx, yy).R < 0.01f && Lighting.GetColor(xx, yy).G < 0.01f && Lighting.GetColor(xx, yy).B < 0.01f)
                            {
                                Asset<Texture2D> SinglePixel = Assets.Textures.Misc.SinglePixel.Asset;
                                Main.EntitySpriteDraw(SinglePixel.Value, new Vector2(xx * 16, yy * 16) - Main.screenPosition, SinglePixel.Frame(), Color.Black, 0f, Vector2.Zero, 16f, SpriteEffects.None);
                            }
                        }
                    }
                }
            }
        }
    }

    public override void Unload()
    {
        On_Main.DoDraw_WallsAndBlacks -= DoDraw_WallsAndBlacks;
    }

    public override string Name => "TheGallery";
    public override int Music => Assets.Sounds.Music.Gallery.Slot;
    public override bool IsBiomeActive(Player player)
    {
        return player.Distance(GallerySystem.GalleryPosition.ToVector2() * 16) < 2300;
    }
}
