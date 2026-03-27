using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Everware.Content.Base.ParticleSystem;

public class TextureSparkParticle : Particle
{
    int timeLeft = 20;
    public bool fadeIn = false;
    public string TextureString;
    public float Decrement = 0.9f;
    public Vector2 origScale = Vector2.One;

    public override Asset<Texture2D> Texture => ModContent.Request<Texture2D>(TextureString);

    public TextureSparkParticle(Vector2 pos, Vector2 vel, float rot, string tex, Vector2 sc, Vector2 orig, Color col, bool add = false, ParticleFunction update = null)
    : base(pos, vel, sc)
    {
        Rotation = rot;
        velocity = vel;
        position = pos;
        Origin = orig;
        Scale = sc;
        TextureString = tex;
        Color = col;
        UpdateFunction = update;
    }

    public TextureSparkParticle(Vector2 pos, Vector2 vel, float rot, string tex, Vector2 sc, Vector2 orig, Color col, float decrement, bool fadeIn = false, bool add = false, ParticleFunction update = null)
    : base(pos, vel, sc)
    {
        Rotation = rot;
        velocity = vel;
        position = pos;
        Origin = orig;
        Scale = sc;
        origScale = sc;
        TextureString = tex;
        Color = col;
        Decrement = decrement;
        UpdateFunction = update;
        this.fadeIn = fadeIn;
        if (fadeIn)
        {
            Scale = Vector2.Zero;
        }
    }

    public override void Update()
    {
        base.Update();
        if (!this.fadeIn)
        {
            velocity *= Decrement;
            Scale *= Decrement;
            if (Scale.Length() < 0.07f)
            {
                Kill();
            }
        }
        if (this.fadeIn)
        {
            ai[2]++;
            if (ai[2] < timeLeft / 2)
            {
                Scale = Vector2.Lerp(Scale, origScale, 0.1f);
            }
            else
            {
                Scale = Vector2.Lerp(Scale, Vector2.Zero, 0.1f);
            }
            if (ai[2] > timeLeft)
            {
                Kill();
            }
        }
    }
}
