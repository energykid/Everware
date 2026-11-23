using Everware.Content.Base.Dusts;

namespace Everware.Content.Meteor.Tiles;

public class MeteorScorchDust : EverAnimatedDust
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Tiles/MeteorScorchDust";
    public override int FrameCount => 4;
    public override Color? GetAlpha(Dust dust, Color lightColor)
    {
        return Color.White.MultiplyRGBA(new(1f, 1f, 1f, 0.5f));
    }

    public override void OnSpawn(Dust dust)
    {
        base.OnSpawn(dust);
        dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }
}