namespace Everware.Content.Base.ParticleSystem;

public class GenericParticle : Particle
{
    public GenericParticle(Vector2 pos, Vector2 vel, Vector2 scale, ParticleFunction upd = null, ParticleFunction drw = null) : base(pos, vel, scale, upd, drw)
    {
        position = pos;
        velocity = vel;
        Scale = scale;
        UpdateFunction = upd;
        DrawFunction = drw;
    }
}
