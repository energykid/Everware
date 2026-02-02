
namespace Everware.Content.Base.ParticleSystem;

public class GenericParticle : Particle
{
    public GenericParticle(string sprite, Vector2 pos, Vector2 vel, Vector2 scale, ParticleFunction upd = null, ParticleFunction drw = null) : base(sprite, pos, vel, scale, upd, drw)
    {
        Sprite = sprite;
        position = pos;
        velocity = vel;
        Scale = scale;
        UpdateFunction = upd;
        DrawFunction = drw;
    }
}
