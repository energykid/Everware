using Everware.Content.Base.Dusts;
using Everware.Utils;
using Microsoft.Xna.Framework;
using Terraria;

namespace Everware.Content.PreHardmode.Kilnstone;

public abstract class PowderDust : EverDust
{
    public override void OnSpawn(Dust dust)
    {
        base.OnSpawn(dust);
        dust.frame = new Rectangle(
            0, 
            Main.rand.Next(3) * 14,
            16, 
            14);
        dust.alpha = 155;
        dust.velocity = new Vector2(Main.rand.NextFloat(1), 0).RotatedByRandom(MathHelper.TwoPi);
        dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        dust.scale = 0.5f;
    }

    public override bool Update(Dust dust)
    {
        base.Update(dust);

        dust.scale = MathHelper.Lerp(dust.scale, 1f, 0.1f);

        dust.fadeIn++;

        dust.velocity *= 0.9f;

        float rot = 0f;
        rot = Easing.KeyFloat(dust.fadeIn, 0f, 60, 0.2f, 0.01f, Easing.OutCirc);
        dust.rotation += rot;

        if (dust.fadeIn > 30)
            dust.velocity.Y += 0.1f;

        if (dust.fadeIn > 40)
            dust.alpha += 10;

        if (dust.alpha >= 255)
            dust.active = false;

        return false;
    }
}

public class KilnPowderDust : PowderDust { }
public class RawKilnPowderDust : PowderDust { }