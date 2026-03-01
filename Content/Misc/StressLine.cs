using Everware.Content.Base.ParticleSystem;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Everware.Content.Misc;


public class StressLine : Particle
{
    float T = 0f;
    public override Asset<Texture2D> Texture => Assets.Textures.Misc.StressLine.Asset;
    public StressLine(Vector2 pos, Vector2 vel) : base(pos, vel, Vector2.One, null, null)
    {
        FrameCount = new(7, 1);
        FrameNum = new(0, 0);
    }
    public override void Update()
    {
        base.Update();
        velocity *= 0.9f;
        Rotation = velocity.AngleFrom(Vector2.Zero);
        T++;
        if (T % 4 == 0)
        {
            FrameNum.X++;
            if (FrameNum.X > 6)
            {
                Kill();
            }
        }
    }
}