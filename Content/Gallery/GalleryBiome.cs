using Everware.Content.Base;
using Terraria.ModLoader.IO;

namespace Everware.Content.Gallery;

public class GallerySystem : ModSystem
{
    public override void Load()
    {
        On_Main.DoLightTiles += LightBackground;
    }

    private void LightBackground(On_Main.orig_DoLightTiles orig, Main self)
    {
        if (Main.LocalPlayer.Distance(GalleryPosition.ToVector2() * 16) < 3000)
        {
            for (int i = -75; i <= 75; i++)
            {
                for (int j = -75; j <= 0; j++)
                {
                    int xx = GalleryPosition.X + i;
                    int yy = GalleryPosition.Y + j;
                    if (xx > 250 && xx < Main.maxTilesX - 250)
                    {
                        if (yy > 250 && yy < Main.maxTilesY - 250)
                        {
                            Tile t = Main.tile[xx, yy];
                            if (!t.HasTile && new Vector2(GalleryPosition.X + i, GalleryPosition.Y + j).Distance(GalleryPosition.ToVector2()) < 75)
                                Lighting.AddLight(GalleryPosition.X + i, GalleryPosition.Y + j, 0.5f, 0.5f, 1f);
                        }
                    }
                }
            }
        }

        orig(self);
    }

    public override void Unload()
    {
        On_Main.DoLightTiles -= LightBackground;
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
        On_Main.DoDraw_WallsAndBlacks += On_Main_DoDraw_WallsAndBlacks;
    }

    private void On_Main_DoDraw_WallsAndBlacks(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self)
    {
        if (!Main.gameInactive)
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
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, ParallaxEffect.Shader, Main.BackgroundViewMatrix.EffectMatrix);

            // Looping background texture in the very back
            Main.spriteBatch.Draw(BGMask.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0.25f, 0.25f, 0.5f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, GalleryClipColor.Shader, Main.BackgroundViewMatrix.EffectMatrix);

            // Middle background
            Main.spriteBatch.Draw(BGMiddle.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0.5f, 0.5f, 1f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, GalleryClipColor.Shader, Main.BackgroundViewMatrix.EffectMatrix);

            GalleryClipColor.Parameters.MultColor = new Color(0f, 0f, 0f, 0.2f).ToVector4();
            GalleryClipColor.Apply();

            // Middle background etchings (shadows)
            Main.spriteBatch.Draw(BGMiddleGlow.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0f, 0f, 0f, 0.2f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, null, null, ShineEffect.Shader, Main.BackgroundViewMatrix.EffectMatrix);

            // Middle background etchings (glows)
            Main.spriteBatch.Draw(BGMiddleGlow.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), Color.White, 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.BackgroundViewMatrix.EffectMatrix);

            // Front background etchings (shadows)
            Main.spriteBatch.Draw(BGFront.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0.5f, 0.5f, 1f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            // Front background etchings (shadows)
            Main.spriteBatch.Draw(BGFrontGlow.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), new Color(0f, 0f, 0f, 0.2f), 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            ShineEffect.Parameters.Parallax = 0f;
            ShineEffect.Apply();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, null, null, ShineEffect.Shader, Main.BackgroundViewMatrix.EffectMatrix);

            // Front background etchings (glows)
            Main.spriteBatch.Draw(BGFrontGlow.Value, (GallerySystem.GalleryPosition.ToVector2() * 16f) - Main.screenPosition, BGMask.Frame(), Color.White, 0f, ORIGIN, 1f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main._multiplyBlendState, Main.DefaultSamplerState, null, null, null, Main.BackgroundViewMatrix.EffectMatrix);
        }
        orig(self);
    }

    public override void Unload()
    {
        On_Main.DoDraw_WallsAndBlacks -= On_Main_DoDraw_WallsAndBlacks;
    }

    public override string Name => "TheGallery";
    public override int Music => Assets.Sounds.Music.Gallery.Slot;
    public override bool IsBiomeActive(Player player)
    {
        return player.Distance(GallerySystem.GalleryPosition.ToVector2() * 16) < 2300;
    }
}
