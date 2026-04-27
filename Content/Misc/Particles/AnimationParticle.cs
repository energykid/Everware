using Everware.Content.Base.ParticleSystem;

namespace Everware.Content.Misc.Particles;

public class AnimationParticle : Particle
{
    float T = 0f;
    public Asset<Texture2D> Asset;
    public override Asset<Texture2D> Texture => Asset;
    public AnimationParticle(Vector2 pos, Vector2 vel, Color color, Asset<Texture2D> asset, int frames) : base(pos, vel, Vector2.One, null, null)
    {
        FrameCount = new(1, frames);
        FrameNum = new(0, 0);
        Pixelated = true;
        Color = color;
        Asset = asset;
    }
    public override void Update()
    {
        AffectedByLight = false;
        base.Update();
        velocity *= 0.9f;
        T++;
        if (T % 4 == 0)
        {
            FrameNum.Y++;
            if (FrameNum.Y >= FrameCount.Y)
            {
                Kill();
            }
        }
    }
}