using Everware.Content.Base.ParticleSystem;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Everware.Content.Misc.Particles;

public class SmallMist : Particle
{
    float T = 0f;
    public override Asset<Texture2D> Texture => Assets.Textures.Misc.SmallMist.Asset;
    public SmallMist(Vector2 pos, Vector2 vel, Color color, Vector2 targetScale) : base(pos, vel, Vector2.One, null, null)
    {
        Color = color;
        AffectedByLight = true;
        Scale = targetScale;
    }
    public override void Update()
    {
        base.Update();
        Opacity *= 0.9f;
        velocity *= 0.9f;
        Rotation += velocity.AngleFrom(Vector2.Zero) / 6f;
        if (Opacity < 0.05f) Kill();
    }
}
public class SmallMistFade : Particle
{
    float T = 0f;
    Vector2 TargetScale = Vector2.One;
    public override Asset<Texture2D> Texture => Assets.Textures.Misc.SmallMist.Asset;
    public SmallMistFade(Vector2 pos, Vector2 vel, Color color, Vector2 targetScale) : base(pos, vel, Vector2.One, null, null)
    {
        Origin = Texture.Size() / 2f;
        Color = color;
        Opacity = 0f;
        Scale = Vector2.Zero;
        AffectedByLight = true;
    }
    public override void Update()
    {
        base.Update();
        Scale = Vector2.Lerp(Scale, TargetScale, 0.1f);
        T = MathHelper.Lerp(T, 1f, 0.1f);
        if (T < 0.5f) 
            Opacity = MathHelper.Lerp(Opacity, 1f, 0.3f);
        else
        {
            Opacity *= 0.9f;
            if (Opacity < 0.05f) Kill();
        }
        velocity *= 0.9f;
        Rotation += velocity.AngleFrom(Vector2.Zero) / 6f;
    }
}