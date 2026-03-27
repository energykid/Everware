using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Everware.Content.Base.ParticleSystem;

public class TexturePulseParticle : Particle
{
    public string TextureString;
    public virtual Asset<Texture2D> Texture => ModContent.Request<Texture2D>(TextureString);
    public float Decrement = 0.9f;

    public Vector2 scaleDestination = Vector2.Zero;

    public TexturePulseParticle(Vector2 pos, Vector2 vel, float rot, string tex, Vector2 startScale, Vector2 endScale, Vector2 orig, Color col, bool add = false, ParticleFunction update = null)
    : base(pos, vel, startScale)
    {
        Rotation = rot;
        velocity = vel;
        position = pos;
        Origin = orig;
        Scale = startScale;
        scaleDestination = endScale;
        TextureString = tex;
        Color = col;
        UpdateFunction = update;
    }

    public TexturePulseParticle(Vector2 pos, Vector2 vel, float rot, string tex, Vector2 startScale, Vector2 endScale, Vector2 orig, Color col, float decrement, bool add = false, ParticleFunction update = null)
    : base(pos, vel, startScale)
    {
        Rotation = rot;
        velocity = vel;
        position = pos;
        Origin = orig;
        Scale = startScale;
        scaleDestination = endScale;
        TextureString = tex;
        Color = col;
        Decrement = decrement;
        UpdateFunction = update;
    }

    public TexturePulseParticle(Vector2 pos, Vector2 vel, Vector2 scale, ParticleFunction upd = null, ParticleFunction drw = null) : base(pos, vel, scale, upd, drw)
    {
    }

    public override void Update()
    {
        base.Update();
        velocity *= Decrement;
        Scale = Vector2.Lerp(Scale, scaleDestination, MathHelper.Lerp(1, 0, Decrement));
        Opacity *= Decrement;
        if (Opacity < 0.05f)
        {
            Kill();
        }
    }
}
