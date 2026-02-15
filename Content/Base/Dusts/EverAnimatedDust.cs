namespace Everware.Content.Base.Dusts;

public abstract class EverAnimatedDust : EverDust
{
    public virtual int FrameSpeed => 4;
    public virtual int FrameCount => 6;
    public override void OnSpawn(Dust dust)
    {
        base.OnSpawn(dust);
        int frameWidth = (int)Texture2D.Size().X / FrameCount;
        dust.frame = new Rectangle(0, 0, frameWidth, (int)Texture2D.Size().Y);
    }
    public override bool Update(Dust dust)
    {
        dust.fadeIn++;
        if (dust.fadeIn > FrameSpeed)
        {
            int frameWidth = (int)Texture2D.Size().X / FrameCount;

            dust.frame.X += frameWidth;

            if (dust.frame.X >= Texture2D.Size().X)
                dust.active = false;

            dust.fadeIn = 0;
        }
        base.Update(dust);
        return false;
    }
}
