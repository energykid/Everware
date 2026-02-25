using Everware.Content.Base.ParticleSystem;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Everware.Content.Quarry.Visual;

public class WeldingSpark : Particle
{
    public override Asset<Texture2D> Texture => ModContent.Request<Texture2D>("Everware/Content/PreHardmode/Quarry/Visual/WeldingSpark");
    public WeldingSpark(Vector2 pos, Vector2 vel) : base(pos, vel, Vector2.One, null, null)
    {
        position = pos;
        velocity = vel;
        AffectedByLight = false;
        Rotation = velocity.AngleFrom(Vector2.Zero);
    }
    public override void Update()
    {
        Scale = new Vector2(velocity.Length() / 2f, 1f);
        Rotation = velocity.AngleFrom(Vector2.Zero);
        velocity.Y += 0.2f;
        Opacity -= 0.08f;
        Lighting.AddLight(position, 0.2f, 0.1f, 0.09f);
        if (Opacity < 0) Kill();
        base.Update();
    }
}
