using Everware.Content.Base.ParticleSystem;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Everware.Content.Hell;

public class HellPodDebris : Particle
{
    public override Asset<Texture2D> Texture => Assets.Textures.Hell.HellPodDebris.Asset;
    public float GlowOpacity = 1f;
    float VelMod = 0.2f;
    float UpTime = 40;
    float t = 0f;
    public HellPodDebris(Vector2 pos, int numFrame, float rotation) : base(pos, Vector2.Zero, Vector2.One, null, null)
    {
        UpTime = Main.rand.Next(40, 60);

        FrameNum = new Vector2(numFrame, 0);

        Vector2 rot = new Vector2(-1.5f, -2f);

        if (numFrame == 1)
            rot = new Vector2(0.75f, 1f);
        if (numFrame == 2)
            rot = new Vector2(-0.85f, -0.2f);
        if (numFrame == 3)
            rot = new Vector2(-0.75f, -0.85f);
        if (numFrame == 4)
            rot = new Vector2(1.5f, 2f);

        velocity = rot.RotatedBy(rotation) * 2f;
        VelMod *= Main.rand.NextFloat(0.8f, 1.5f);
        FrameCount = new Vector2(5, 1);
        UpdateFunction = Particle =>
        {
            t++;
            Rotation *= 0.98f;
            GlowOpacity *= 0.9f;
            Particle.velocity.X *= 0.85f;
            if (t > UpTime)
            {
                Particle.velocity.Y = Easing.KeyFloat(t, UpTime, UpTime + 20, 0f, -12f, Easing.InBack);
                Particle.Scale = Easing.KeyVector2(t, UpTime, UpTime + 20, Vector2.One, new Vector2(0f, 2f), Easing.InBack);
                if (t > UpTime + 20) Particle.Kill();
            }
            else
            {
                Particle.velocity.Y *= 0.85f;
                Particle.velocity.Y += MathHelper.Lerp(VelMod / 2f, 0f, t / UpTime);
            }
        };
    }
    public override void Draw()
    {
        base.Draw();

        if (GlowOpacity > 0.6f)
        {
            var tex2 = Assets.Textures.Hell.HellPodDebrisBrightGlow.Asset;

            Color c = !AffectedByLight ? Color : Color.MultiplyRGBA(Lighting.GetColor((position / 16f).ToPoint()));
            Rectangle frame = Texture.Frame((int)FrameCount.X, (int)FrameCount.Y, (int)FrameNum.X, (int)FrameNum.Y);
            Main.EntitySpriteDraw(tex2.Value, position - Main.screenPosition, frame, Color.White, Rotation, Origin != -Vector2.One ? Origin : frame.Size() / 2f, Scale, Effects);
        }
    }
}

public class HellPodBaseDebris : Particle
{
    public override Asset<Texture2D> Texture => Assets.Textures.Hell.HellPodBaseDebris.Asset;
    public HellPodBaseDebris(Vector2 pos, Vector2 vel, Vector2 scale) : base(pos, vel, scale, null, null)
    {
        FrameCount = new Vector2(4, 1);
        FrameNum = new Vector2(Main.rand.Next(4), 0);
        UpdateFunction = Particle =>
        {
            Particle.Rotation += MathHelper.ToRadians(Particle.velocity.X);
            Particle.velocity.Y += 0.1f;
            Particle.velocity.X *= 0.89f;
            if (Particle.velocity.Y > 2)
            {
                Particle.Opacity = MathHelper.Lerp(Particle.Opacity, -0.1f, 0.1f);
                if (Particle.Opacity < 0) Particle.Kill();
            }
        };
    }
}


public class HellPodStalkDebris : Particle
{
    public override Asset<Texture2D> Texture => Assets.Textures.Hell.HellPodStalkDebris.Asset;
    public int FrameDelay = 0;
    public float Rotator = 0f;
    public HellPodStalkDebris(Vector2 pos, float rot) : base(pos, Vector2.Zero, Vector2.One, null, null)
    {
        velocity = Vector2.Zero;
        Rotation = rot;
        Rotator = Main.rand.NextFloat(-0.06f, 0.06f);
        FrameCount = new Vector2(8, 2);
        UpdateFunction = Particle =>
        {
            Color = Lighting.GetColor((new Vector2(position.X + 8, position.Y + 8) / 16).ToPoint());

            FrameDelay--;

            if (FrameDelay == -12)
            {
                velocity = new Vector2(Main.rand.NextFloat(4), 0).RotatedByRandom(MathHelper.TwoPi);
            }
            if (FrameDelay < 0)
            {
                Rotation += Rotator;
                velocity.Y += 0.05f;
                velocity.X *= 0.95f;
            }

            if (FrameDelay < 0 && FrameDelay % 6 == 0)
            {
                FrameNum.X += 2;
                if (FrameNum.X >= 8) Particle.Kill();
            }

            if (WorldGen.SolidTile(Main.tile[(position / 16).ToPoint()])) Particle.Kill();
        };
    }
}

public class HellPodPopShockwave : Particle
{
    float T = 0f;
    public HellPodPopShockwave(Vector2 pos) : base(pos, Vector2.Zero, Vector2.One, null, null)
    {
        T = 0f;
    }
    public override void Update()
    {
        base.Update();
        T = MathHelper.Lerp(T, 1.1f, 0.18f);
        if (T > 1f) Kill();
    }
    public override void Draw()
    {
        var A = Assets.Textures.Hell.HellPodPopShockwave.Asset;
        var A2 = Assets.Textures.Hell.HellPodBubble.Asset;

        var HellPodEffect = Assets.Effects.Misc.GradientClip.CreateEffect();
        HellPodEffect.Parameters.ColorClip = 0 - T;
        HellPodEffect.Parameters.ColorClipUpper = 1 - T;
        HellPodEffect.Parameters.Gradient = Assets.Textures.Hell.HellPodPaletteGradient.Asset.Value;
        HellPodEffect.Parameters.LightingColor = Color.White.ToVector4();
        HellPodEffect.Apply();

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, null, Main.Rasterizer, HellPodEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

        for (int i = 0; i < 4; i++)
        {
            Main.spriteBatch.Draw(A.Value, position + new Vector2(2, 0).RotatedBy(i * MathHelper.PiOver2) - Main.screenPosition, A.Frame(), Color.White, 0f, A.Size() / 2f, 1f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(A2.Value, position + new Vector2(2, 0).RotatedBy(i * MathHelper.PiOver2) - Main.screenPosition, A2.Frame(), Color.White, 0f, A2.Size() / 2f, 1f, SpriteEffects.None, 0);
        }

        Main.spriteBatch.Draw(A.Value, position - Main.screenPosition, A.Frame(), Color.White, 0f, A.Size() / 2f, 1f, SpriteEffects.None, 0);

        Main.spriteBatch.Draw(A2.Value, position - Main.screenPosition, A2.Frame(), Color.White, 0f, A2.Size() / 2f, 1f, SpriteEffects.None, 0);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
    }
}