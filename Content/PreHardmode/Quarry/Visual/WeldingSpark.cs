using Everware.Content.Base.ParticleSystem;

namespace Everware.Content.PreHardmode.Quarry.Visual;

public class WeldingSpark : Particle
{
    public static new string Sprite => "Everware/Content/PreHardmode/Quarry/Visual/WeldingSpark";
    public WeldingSpark(Vector2 pos, Vector2 vel) : base(pos, vel, Vector2.One, null, null)
    {
        AffectedByLight = false;
    }
    public override void Update()
    {
        base.Update();
        Scale = new Vector2(velocity.Length() / 4f, 1f);
        Rotation = velocity.AngleFrom(Vector2.Zero);
        velocity.Y += 0.1f;
    }
}
