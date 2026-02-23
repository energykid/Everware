using Everware.Content.Base.ParticleSystem;

namespace Everware.Content.Misc;

public class StressLine : Particle
{
    public StressLine(Vector2 pos, Vector2 vel) : base(pos, vel, Vector2.One, null, null)
    {
    }
    public override void Update()
    {
        base.Update();

    }
}
