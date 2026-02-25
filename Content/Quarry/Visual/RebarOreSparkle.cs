using Everware.Content.Base.Dusts;

namespace Everware.Content.Quarry.Visual;

public class RebarOreSparkle : EverAnimatedDust
{
    public override string Texture => "Everware/Assets/Textures/Quarry/RebarOreSparkle";
    public override int FrameCount => 7;
    public override int FrameSpeed => 1;
    public override void OnSpawn(Dust dust)
    {
        base.OnSpawn(dust);
        dust.velocity = new Vector2(Main.rand.NextFloat(4), 0).RotatedByRandom(MathHelper.TwoPi);
    }
    public override bool Update(Dust dust)
    {
        base.Update(dust);

        dust.scale = MathHelper.Lerp(dust.scale, 1f, 0.3f);

        dust.velocity *= 0.85f;

        return false;
    }
}