using Everware.Content.Base.ParticleSystem;

public class SnapdragonFrostBreathParticle : Particle
{
    public Asset<Texture2D> MyTexture = null;
    public override Asset<Texture2D> Texture => MyTexture;
    public SnapdragonFrostBreathParticle(Vector2 pos, Vector2 vel, Vector2 scale, Asset<Texture2D> asset) : base(pos, vel, scale, null, null)
    {
        MyTexture = asset;
    }
    public override Vector2 VisualPosition => position;
}