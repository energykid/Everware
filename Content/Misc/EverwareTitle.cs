using Everware.Core;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;

namespace Everware.Content.Misc;

public class EverwareTitle : ModMenu
{
    public static Vector2 Parallax;
    public static RenderTarget2D Target;
    public static float UpdateTimer = 0;
    public override int Music => Sounds.Music.SomewhereElse.Slot;
    public override string DisplayName => "Somewhere Else";
    public override Asset<Texture2D> Logo => AssetReferences.Content.Misc.Logo.Asset;
    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
    {
        Parallax = Vector2.Lerp(Parallax, Main.MouseScreen * 0.03f, 0.05f);

        Main.time = Main.dayLength / 2;

        UpdateTimer++;

        var Background = AssetReferences.Content.Misc.MenuArtBackground.Asset;
        var Foreground = AssetReferences.Content.Misc.MenuArtForeground.Asset;
        var Mist1 = AssetReferences.Content.Misc.MenuLayer1.Asset;
        var Mist2 = AssetReferences.Content.Misc.MenuLayer2.Asset;
        var PartialVignette = AssetReferences.Content.Misc.MenuLayer4.Asset;
        var Outline = AssetReferences.Content.Misc.MenuLayer3.Asset;

        Color MistColorForeground = Color.DarkSlateBlue;
        Color MistColor1Background = Color.CadetBlue;
        Color MistColor2Background = Color.CadetBlue;

        var NoiseTex = AssetReferences.Content.Misc.MenuNoise.Asset;

        float BackgroundTime = (UpdateTimer / 8 % 1920f);
        float BackgroundTime2 = (float)(Math.Floor(-((UpdateTimer * 1.4f) % 1080f) / 2) * 2);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.UIScaleMatrix);

        Main.spriteBatch.Draw(Background.Value, Vector2.Zero + (Parallax * 0.3f), Background.Frame(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        var OutlineEffect = AssetReferences.Content.Misc.PureColorEffect.CreateColorEffect();

        OutlineEffect.Parameters.MultiplyColor = Color.White.ToVector4();
        OutlineEffect.Apply();

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, OutlineEffect.Shader, Main.UIScaleMatrix);

        for (int i = 0; i < 8; i++)
        {
            Main.spriteBatch.Draw(Foreground.Value, new Vector2(Main.screenWidth * 0.75f, Main.screenHeight * 0.65f) + Parallax + new Vector2(4, 0).RotatedBy(MathHelper.ToRadians(45f) * i), Foreground.Frame(), Color.White, 0f, Foreground.Frame().Size() / 2f, 1f, SpriteEffects.None, 0f);
        }

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.UIScaleMatrix);

        Main.spriteBatch.Draw(Foreground.Value, new Vector2(Main.screenWidth * 0.75f, Main.screenHeight * 0.65f) + Parallax, Foreground.Frame(), Color.White, 0f, Foreground.Frame().Size() / 2f, 1f, SpriteEffects.None, 0f);

        Main.spriteBatch.Draw(PartialVignette.Value, Vector2.Zero, PartialVignette.Frame(), Color.White, 0f, Vector2.Zero, new Vector2(1f / PartialVignette.Width() * Main.screenWidth, 1f / PartialVignette.Height() * Main.screenHeight), SpriteEffects.None, 0f);

        var Effect2 = AssetReferences.Content.Misc.EverwareMenuMistEffect.CreateWiggleEffect();

        Effect2.Parameters.Timer = -UpdateTimer / 1000f;
        Effect2.Parameters.NoiseTexture = NoiseTex.Value;
        Effect2.Parameters.TextureSize = new Vector2(Main.screenWidth, Main.screenHeight);
        Effect2.Parameters.NoiseTextureSize = NoiseTex.Size();
        Effect2.Parameters.GloobLength = 50f;
        Effect2.Parameters.MultiplyColor = new Color(24, 24, 73).ToVector4();
        Effect2.Parameters.Glow = true;
        Effect2.Apply();

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, null, Main.Rasterizer, Effect2.Shader, Main.UIScaleMatrix);

        for (int i = 0; i < 5; i++)
            Main.spriteBatch.Draw(Mist1.Value, Vector2.Zero, Mist1.Frame(), MistColorForeground, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

        spriteBatch.End();

        var Effect = AssetReferences.Content.Misc.EverwareMenuMistEffect.CreateWiggleEffect();

        Effect.Parameters.Timer = -UpdateTimer / 1000f;
        Effect.Parameters.NoiseTexture = NoiseTex.Value;
        Effect.Parameters.TextureSize = new Vector2(Main.screenWidth, Main.screenHeight);
        Effect.Parameters.NoiseTextureSize = NoiseTex.Size();
        Effect.Parameters.GloobLength = 50f;
        Effect.Parameters.MultiplyColor = new Color(0f, 0f, 0f, 0.3f).ToVector4();
        Effect.Parameters.Glow = false;
        Effect.Apply();

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, Effect.Shader, Main.UIScaleMatrix);

        Main.spriteBatch.Draw(Mist1.Value, Vector2.Zero, Mist1.Frame(), MistColorForeground, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.UIScaleMatrix);

        Main.spriteBatch.Draw(Outline.Value, Vector2.Zero, Outline.Frame(), MistColorForeground, 0f, Vector2.Zero, new Vector2(1f / Outline.Width() * Main.screenWidth, 1f / Outline.Height() * Main.screenHeight), SpriteEffects.None, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.UIScaleMatrix);

        return true;
    }
}