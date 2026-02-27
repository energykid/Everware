using Everware.Common.Systems;
using Everware.Content.Base.ParticleSystem;

namespace Everware.Content.EyeOfCthulhuRework;

public class CthulhuBloodParticle : Particle
{
    int A = 0;
    float Clip = 0f;
    float ClipUpper = 0.6f;

    public CthulhuBloodParticle(Vector2 pos, Vector2 vel) : base(pos, vel, Vector2.One, null, null)
    {
        Rotation = vel.ToRotation() + MathHelper.PiOver2;
    }

    public override void Update()
    {
        A++;
        if (A > 3)
        {
            Clip = MathHelper.Lerp(Clip, 1.02f, 0.15f);
            ClipUpper = MathHelper.Lerp(ClipUpper, 1.02f, 0.4f);
            velocity *= 0.9f;
            base.Update();
            if (Clip >= 1f) Kill();
        }
    }
    public override void Draw()
    {
        var BloodEffect = Assets.Effects.EyeOfCthulhu.CthulhuBlood.CreateBloodEffect();

        var asset = Assets.Textures.EyeOfCthulhu.EyeOfCthulhu_BloodMask.Asset;

        PixelRendering.DrawPixelatedSprite(asset.Value,
            position - Main.screenPosition, asset.Frame(), Color.White, Rotation, asset.Size() / 2, Scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
            effect: BloodEffect.Shader, setparams: () =>
            {
                BloodEffect.Parameters.BloodGradient = Assets.Textures.EyeOfCthulhu.BloodPalette.Asset.Value;
                BloodEffect.Parameters.LightingColor = Lighting.GetColor((position / 16f).ToPoint()).ToVector4();
                BloodEffect.Parameters.ColorClip = Clip;
                BloodEffect.Parameters.ColorClipUpper = ClipUpper;
                BloodEffect.Apply();
            });
    }
}

public class CthulhuBloodRingParticle : Particle
{
    int A = 0;
    float Clip = 0f;
    float ClipUpper = 1f;

    public CthulhuBloodRingParticle(Vector2 pos, Vector2 vel) : base(pos, vel, Vector2.One, null, null)
    {
        Rotation = vel.ToRotation() + MathHelper.PiOver2;
    }

    public override void Update()
    {
        A++;
        if (A > 3)
        {
            Clip = MathHelper.Lerp(Clip, 1.02f, 0.15f);
            velocity *= 0.9f;
            base.Update();
            if (Clip >= 1f) Kill();
        }
    }
    public override void Draw()
    {
        var BloodEffect = Assets.Effects.EyeOfCthulhu.CthulhuBlood.CreateBloodEffect();

        var asset = Textures.SkewedRadialBlast.Asset;

        PixelRendering.DrawPixelatedSprite(asset.Value,
            position - Main.screenPosition, asset.Frame(), Color.White, Rotation, asset.Size() / 2, Scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
            effect: BloodEffect.Shader, setparams: () =>
            {
                BloodEffect.Parameters.BloodGradient = Assets.Textures.EyeOfCthulhu.BloodPalette.Asset.Value;
                BloodEffect.Parameters.LightingColor = Lighting.GetColor((position / 16f).ToPoint()).ToVector4();
                BloodEffect.Parameters.ColorClip = Clip;
                BloodEffect.Parameters.ColorClipUpper = ClipUpper;
                BloodEffect.Apply();
            });
    }
}
