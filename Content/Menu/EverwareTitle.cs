using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;

namespace Everware.Content.Menu;

public class EverwareTitle : ModMenu
{
    public static Vector2 Parallax;
    public static RenderTarget2D Target;
    public static float UpdateTimer = 0;
    public override int Music => Assets.Sounds.Music.SomewhereElse.Slot;
    public override string DisplayName => "Somewhere Else";
    public override Asset<Texture2D> Logo => Assets.Textures.Menu.Logo.Asset;
    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
    {
        var LogoEffect = Assets.Effects.Menu.EverwareLogoEffect.CreateLogoEffect();
        LogoEffect.Parameters.Timer = -UpdateTimer / 1000f;
        LogoEffect.Parameters.TextResolution = Assets.Textures.Menu.Logo.Asset.Size();
        LogoEffect.Parameters.FillResolution = Assets.Textures.Menu.LogoFill.Asset.Size();
        LogoEffect.Parameters.FillTexture = Assets.Textures.Menu.LogoFill.Asset.Value;
        LogoEffect.Apply();

        Parallax = Vector2.Lerp(Parallax, Main.MouseScreen / Main.ScreenSize.ToVector2(), 0.05f);

        Main.time = Main.dayLength / 2;

        UpdateTimer++;

        var Background = Assets.Textures.Menu.MenuBackground.Asset;
        var Foreground = Assets.Textures.Menu.MenuArtForeground.Asset;
        var Mist = Assets.Textures.Menu.MenuLayer1.Asset;
        var PartialVignette = Assets.Textures.Menu.MenuLayer2.Asset;
        var Outline = Assets.Textures.Menu.MenuLayer3.Asset;
        var Logo = Assets.Textures.Menu.Logo.Asset;
        var LogoMask = Assets.Textures.Menu.LogoMask.Asset;

        Color MistColorForeground = Color.DarkSlateBlue;
        Color MistColor1Background = Color.CadetBlue;
        Color MistColor2Background = Color.CadetBlue;

        var NoiseTex = Assets.Textures.Menu.MenuNoise.Asset;

        float BackgroundTime = (UpdateTimer / 8 % 1920f);
        float BackgroundTime2 = (float)(Math.Floor(-((UpdateTimer * 1.4f) % 1080f) / 2) * 2);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.UIScaleMatrix);

        Main.spriteBatch.Draw(Background.Value, new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f) + (Parallax * 10f), Background.Frame(), Color.White, 0f, Background.Frame().Size() / 2f, 1f, SpriteEffects.None, 0f);

        var OutlineEffect = Assets.Effects.Misc.PureColorEffect.CreateColorEffect();

        OutlineEffect.Parameters.MultiplyColor = Color.White.ToVector4();
        OutlineEffect.Apply();

        /*
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, OutlineEffect.Shader, Main.UIScaleMatrix);

        for (int i = 0; i < 8; i++)
        {
            Main.spriteBatch.Draw(Foreground.Value, new Vector2(Main.screenWidth * 0.75f, Main.screenHeight * 0.65f) + Parallax + new Vector2(4, 0).RotatedBy(MathHelper.ToRadians(45f) * i), Foreground.Frame(), Color.White, 0f, Foreground.Frame().Size() / 2f, 1f, SpriteEffects.None, 0f);
        }
        */

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.UIScaleMatrix);

        /*
        Main.spriteBatch.Draw(Foreground.Value, new Vector2(Main.screenWidth * 0.75f, Main.screenHeight * 0.65f) + Parallax, Foreground.Frame(), Color.White, 0f, Foreground.Frame().Size() / 2f, 1f, SpriteEffects.None, 0f);
        */

        Main.spriteBatch.Draw(PartialVignette.Value, Vector2.Zero, PartialVignette.Frame(), Color.White, 0f, Vector2.Zero, new Vector2(1f / PartialVignette.Width() * Main.screenWidth, 1f / PartialVignette.Height() * Main.screenHeight), SpriteEffects.None, 0f);

        var Effect2 = Assets.Effects.Menu.EverwareMenuMistEffect.CreateWiggleEffect();

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
            Main.spriteBatch.Draw(Mist.Value, Vector2.Zero - new Vector2(20, 20) + (Parallax * 20), Mist.Frame(), MistColorForeground, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

        spriteBatch.End();

        var Effect = Assets.Effects.Menu.EverwareMenuMistEffect.CreateWiggleEffect();

        Effect.Parameters.Timer = -UpdateTimer / 1000f;
        Effect.Parameters.NoiseTexture = NoiseTex.Value;
        Effect.Parameters.TextureSize = new Vector2(Main.screenWidth, Main.screenHeight);
        Effect.Parameters.NoiseTextureSize = NoiseTex.Size();
        Effect.Parameters.GloobLength = 50f;
        Effect.Parameters.MultiplyColor = new Color(0f, 0f, 0f, 0.3f).ToVector4();
        Effect.Parameters.Glow = false;
        Effect.Apply();

        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, Effect.Shader, Main.UIScaleMatrix);

        Main.spriteBatch.Draw(Mist.Value, Vector2.Zero - new Vector2(20, 20) + (Parallax * 20), Mist.Frame(), MistColorForeground, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.UIScaleMatrix);

        Main.spriteBatch.Draw(Outline.Value, Vector2.Zero, Outline.Frame(), MistColorForeground, 0f, Vector2.Zero, new Vector2(1f / Outline.Width() * Main.screenWidth, 1f / Outline.Height() * Main.screenHeight), SpriteEffects.None, 0f);

        Main.spriteBatch.Draw(Logo.Value, new Vector2(Main.screenWidth / 2f, 110), Logo.Frame(), Color.White, logoRotation, Logo.Size() / 2f, new Vector2(logoScale), SpriteEffects.None, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Main.Rasterizer, LogoEffect.Shader, Main.UIScaleMatrix);

        Main.spriteBatch.Draw(LogoMask.Value, new Vector2(Main.screenWidth / 2f, 110), Logo.Frame(), Color.White, logoRotation, Logo.Size() / 2f, new Vector2(logoScale), SpriteEffects.None, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.UIScaleMatrix);

        return false;
    }
}