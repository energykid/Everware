using Everware.Content.Base.ParticleSystem;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Everware.Content.Misc;

public class SmallSmoke : Particle
{
    float T = 0f;
    public override Asset<Texture2D> Texture => Assets.Textures.Misc.SmallSmoke.Asset;
    public SmallSmoke(Vector2 pos, Vector2 vel, Color color) : base(pos, vel, Vector2.One, null, null)
    {
        FrameCount = new(1, 5);
        FrameNum = new(0, 0);
        Pixelated = true;
        Color = color;
        AffectedByLight = true;
    }
    public override void Update()
    {
        base.Update();
        Opacity *= 0.9f;
        velocity.Y -= 0.2f;
        velocity *= 0.9f;
        Rotation = velocity.AngleFrom(Vector2.Zero);
        T++;
        if (T % 4 == 0)
        {
            FrameNum.Y++;
            if (FrameNum.Y > 4)
            {
                Kill();
            }
        }
    }
}