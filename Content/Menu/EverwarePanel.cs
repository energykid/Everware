using Daybreak.Common.Features.ModPanel;
using Everware.Utils;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Everware.Content.Menu;

public class EverwarePanel : ModPanelStyle
{
    public struct BubbleDraw(Vector2 position, int frame, float scale)
    {
        public Vector2 Position { get; set; } = position;
        public int Frame { get; set; } = frame;
        public float Scale { get; set; } = scale;
    }

    public class ModIcon() : UIImage(TextureAssets.MagicPixel)
    {
        int Timer = 0;
        public override void Update(GameTime gameTime)
        {
            Timer++;

            base.Update(gameTime);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            List<BubbleDraw> Draws = [];

            var dims = GetDimensions().ToRectangle();
            var center = dims.Center();

            for (float i = 0f; i < 1f; i += 0.333333332f)
            {
                float t = (Timer / 360f) % 1f;

                float A = (t + i) % 1f;

                Vector2 v = new Vector2(20, -20) * A;
                v += new Vector2((float)Math.Sin(A * 8f) * 5f);
                v.X *= 0.6f;

                float k = 0f;
                k = Easing.KeyFloat(A, 0f, 0.2f, 1f, 0f, Easing.OutCirc, k);
                k = Easing.KeyFloat(A, 0.2f, 1f, 0f, 1f, Easing.Linear, k);

                float sc = 1f;
                sc = Easing.KeyFloat(A, 0.7f, 1f, 1f, 0f, Easing.Linear, sc);

                v *= MathHelper.Lerp(2f, 1f, sc);

                Vector2 pos = new Vector2(22, -5) + v;
                int Frame = (int)Math.Floor(k * 7);

                Draws.Add(new BubbleDraw(pos, Frame, sc));
            }

            spriteBatch.End(out var sb);

            DrawStuff(Draws, spriteBatch, sb, 0.6f, 0.1f);
            DrawStuff(Draws, spriteBatch, sb, 0.7f, 0.1f);
            DrawStuff(Draws, spriteBatch, sb, 0.8f, 0.1f);
            DrawStuff(Draws, spriteBatch, sb, 0.9f, 0.1f);
            DrawStuff(Draws, spriteBatch, sb, 0f, 0.2f);
            DrawStuff(Draws, spriteBatch, sb, 0.2f, 0.2f);
            DrawStuff(Draws, spriteBatch, sb, 0.4f, 0.2f);

            DrawStuff(Draws, spriteBatch, sb, 0f, 1f, true);

            spriteBatch.Begin(sb);
        }
        void DrawStuff(List<BubbleDraw> Draws, SpriteBatch spriteBatch, SpriteBatchSnapshot sb, float amount, float range = 0.1f, bool grayscaleOnly = false)
        {
            float a = Easing.InOutBack(HoverAmount);
            float Scale = 0.9f + ((float)Math.Sin(Timer / 100f) * 0.1f) + (a * 0.1f);
            float Rotation = ((float)Math.Sin(Timer / 113f) * 0.1f);

            var dims = GetDimensions().ToRectangle();
            Vector2 center = dims.Center() + new Vector2(40, 0);

            Asset<Texture2D> BubblingInfinity = Assets.Textures.Menu.BubblingInfinity.Asset;
            Asset<Texture2D> CosmicBubble = Assets.Textures.Menu.CosmicBubble.Asset;

            var eff = Assets.Effects.Menu.LogoLuminosityClip.CreateEffect();
            eff.Parameters.ColorClip = amount;
            eff.Parameters.ColorClipUpper = amount + range + 0.001f;
            eff.Parameters.Offset = Vector2.Zero;
            eff.Parameters.Timer = Timer / 200f;
            eff.Parameters.Frame = 1f;
            eff.Parameters.FrameCount = 1f;
            eff.Parameters.SmallSpeedModifier = 1f;
            eff.Parameters.TextResolution = BubblingInfinity.Size();
            eff.Parameters.FillResolution = Assets.Textures.Menu.LogoFill.Asset.Size();
            eff.Parameters.FillTexture = Assets.Textures.Menu.LogoFill.Asset.Value;
            eff.Parameters.GrayscaleOnly = grayscaleOnly;
            eff.Apply();

            var eff2 = Assets.Effects.Menu.LogoLuminosityClip.CreateEffect();
            eff2.Parameters.ColorClip = amount;
            eff2.Parameters.ColorClipUpper = amount + range + 0.001f;
            eff2.Parameters.Offset = Vector2.Zero;
            eff2.Parameters.Timer = Timer / 200f;
            eff2.Parameters.Frame = 1f;
            eff2.Parameters.FrameCount = 7f;
            eff2.Parameters.SmallSpeedModifier = 0.1f;
            eff2.Parameters.TextResolution = BubblingInfinity.Size();
            eff2.Parameters.FillResolution = Assets.Textures.Menu.LogoFill.Asset.Size();
            eff2.Parameters.FillTexture = Assets.Textures.Menu.LogoFill.Asset.Value;
            eff2.Parameters.GrayscaleOnly = grayscaleOnly;
            eff2.Apply();

            foreach (BubbleDraw Draw in Draws)
            {
                Rectangle fr = CosmicBubble.Frame(1, 7, 0, Draw.Frame);

                spriteBatch.Begin(sb with { CustomEffect = eff2.Shader, SamplerState = SamplerState.PointClamp });

                spriteBatch.Draw(CosmicBubble.Value, center + (Draw.Position * Scale).RotatedBy(Rotation), fr, Color.White, Rotation, fr.Size() / 2f, Scale * Draw.Scale, SpriteEffects.None, 0f);

                spriteBatch.End();
            }

            eff.Parameters.Offset = Vector2.Zero;
            eff.Parameters.TextResolution = BubblingInfinity.Size();
            eff.Apply();

            spriteBatch.Begin(sb with { CustomEffect = eff.Shader, SamplerState = SamplerState.PointClamp });

            spriteBatch.Draw(BubblingInfinity.Value, center, BubblingInfinity.Frame(), Color.White, Rotation, BubblingInfinity.Size() / 2f, Scale, SpriteEffects.None, 0f);

            spriteBatch.End();
        }
    }

    public override UIImage? ModifyModIcon(UIPanel element, UIImage modIcon, ref int modIconAdjust)
    {
        return new ModIcon();
    }

    public override Color ModifyEnabledTextColor(bool enabled, Color color)
    {
        color = new Color(140, 161, 255);

        color = Color.Lerp(color, Color.FloralWhite, HoverAmount);

        return base.ModifyEnabledTextColor(enabled, color);
    }

    public bool Hovered = false;
    public static float HoverAmount = 0f;
    public override bool PreDraw(UIPanel element, SpriteBatch sb)
    {
        HoverAmount = MathHelper.Clamp(MathHelper.Lerp(HoverAmount, Hovered ? 1.5f : -0.5f, 0.1f), 0f, 1f);

        return base.PreDraw(element, sb);
    }
    public override bool PreDrawReloadRequiredText(UIPanel element)
    {
        return false;
    }
    public override bool PreDrawModStateTextPanel(UIElement self, bool enabled)
    {
        return false;
    }
    public override bool PreSetHoverColors(UIPanel element, bool hovered)
    {
        Hovered = hovered;

        return base.PreSetHoverColors(element, hovered);
    }
    public float UpdateTimer = 0f;
    public static Vector2 pos = Vector2.Zero;
    public static Vector2 size = Vector2.Zero;
    public override bool PreDrawPanel(UIPanel element, SpriteBatch spriteBatch, ref bool drawDivider)
    {

        element.BackgroundColor = Color.DarkSlateBlue;
        element.BorderColor = new Color(140, 161, 255);

        element.BackgroundColor = Color.Lerp(element.BackgroundColor, new Color(140, 161, 255), HoverAmount);
        element.BorderColor = Color.Lerp(element.BorderColor, Color.FloralWhite, HoverAmount);

        drawDivider = false;

        var asset = Assets.Textures.Menu.ModPanel.Asset;

        element._cornerSize = 26;

        var Effect = Assets.Effects.Menu.PanelGradient.CreateEffect();
        Effect.Parameters.Color = [Color.Black.ToVector4(), Color.DarkSlateBlue.ToVector4(), Color.BlueViolet.ToVector4()];
        Effect.Parameters.MultColor = Color.BlueViolet.ToVector4();
        Effect.Apply();

        spriteBatch.End(out var sb);
        spriteBatch.Begin(sb with { CustomEffect = Effect.Shader });

        DrawPanel(element, spriteBatch, asset.Value, 1f);

        spriteBatch.Restart(sb with { BlendState = BlendState.Additive });

        DrawPanel(element, spriteBatch, asset.Value, Easing.InOutBack(HoverAmount));

        spriteBatch.Restart(sb);

        return false;
    }

    public void DrawPanel(UIPanel element, SpriteBatch spriteBatch, Texture2D texture, float alpha)
    {
        CalculatedStyle dimensions = element.GetDimensions();

        Color color = Color.White.MultiplyRGBA(new(alpha, alpha, alpha, alpha));

        float a = (float)Math.Floor(Easing.InOutBack(HoverAmount));
        dimensions.X -= a * 2f;
        dimensions.Y -= a * 2f;
        dimensions.Width += a * 4f;
        dimensions.Height += a * 4f;
        Point point = new Point((int)dimensions.X, (int)dimensions.Y);
        Point point2 = new Point(point.X + (int)dimensions.Width - element._cornerSize, point.Y + (int)dimensions.Height - element._cornerSize);
        int width = point2.X - point.X - element._cornerSize;
        int height = point2.Y - point.Y - element._cornerSize;
        spriteBatch.Draw(texture, new Rectangle(point.X, point.Y, element._cornerSize, element._cornerSize), new Rectangle(0, 0, element._cornerSize, element._cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y, element._cornerSize, element._cornerSize), new Rectangle(element._cornerSize + element._barSize, 0, element._cornerSize, element._cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X, point2.Y, element._cornerSize, element._cornerSize), new Rectangle(0, element._cornerSize + element._barSize, element._cornerSize, element._cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, element._cornerSize, element._cornerSize), new Rectangle(element._cornerSize + element._barSize, element._cornerSize + element._barSize, element._cornerSize, element._cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + element._cornerSize, point.Y, width, element._cornerSize), new Rectangle(element._cornerSize, 0, element._barSize, element._cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + element._cornerSize, point2.Y, width, element._cornerSize), new Rectangle(element._cornerSize, element._cornerSize + element._barSize, element._barSize, element._cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X, point.Y + element._cornerSize, element._cornerSize, height), new Rectangle(0, element._cornerSize, element._cornerSize, element._barSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y + element._cornerSize, element._cornerSize, height), new Rectangle(element._cornerSize + element._barSize, element._cornerSize, element._cornerSize, element._barSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + element._cornerSize, point.Y + element._cornerSize, width, height), new Rectangle(element._cornerSize, element._cornerSize, element._barSize, element._barSize), color);
    }
}
