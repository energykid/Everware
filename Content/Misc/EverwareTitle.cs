using Everware.Core;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Everware.Content.Misc;

public class EverwareTitle : ModMenu
{
    public static RenderTarget2D Target;
    public static float UpdateTimer = 0;
    public override string DisplayName => "Somewhere Else";
    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
    {
        Main.time = Main.dayLength / 2;

        UpdateTimer++;

        var CrossMark = AssetReferences.Content.Misc.MenuCrossMark.Asset;
        var Mist1 = AssetReferences.Content.Misc.MenuLayer1.Asset;
        var Mist2 = AssetReferences.Content.Misc.MenuLayer2.Asset;

        Color MistColorForeground = Color.DarkSlateBlue;
        Color MistColor1Background = Color.CadetBlue;
        Color MistColor2Background = Color.CadetBlue;

        var NoiseTex = AssetReferences.Content.Misc.MenuNoise.Asset;

        float BackgroundTime = (UpdateTimer / 8 % 1920f);
        float BackgroundTime2 = (float)(Math.Floor(-((UpdateTimer * 1.4f) % 1080f) / 2) * 2);

        Main.spriteBatch.Draw(CrossMark.Value, Vector2.Zero + new Vector2(BackgroundTime, 0), CrossMark.Frame(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(CrossMark.Value, Vector2.Zero + new Vector2(BackgroundTime - 1920, 0), CrossMark.Frame(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);


        var Effect2 = AssetReferences.Content.Misc.EverwareMenuMistEffect.CreateWiggleEffect();

        Effect2.Parameters.Timer = -UpdateTimer / 1000f;
        Effect2.Parameters.NoiseTexture = NoiseTex.Value;
        Effect2.Parameters.TextureSize = new Vector2(Main.screenWidth, Main.screenHeight);
        Effect2.Parameters.NoiseTextureSize = NoiseTex.Size();
        Effect2.Parameters.GloobLength = 50f;
        Effect2.Parameters.MultiplyColor = Color.SlateBlue.ToVector4();
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
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.UIScaleMatrix);

        return true;
    }
}