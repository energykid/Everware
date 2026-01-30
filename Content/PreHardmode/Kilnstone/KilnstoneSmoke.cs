using Everware.Content.Base.Dusts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Everware.Content.PreHardmode.Kilnstone;

public class KilnstoneSmoke : EverAnimatedDust
{
    public override int FrameCount => 6;
    public override int FrameSpeed => 5;
    public override bool Update(Dust dust)
    {
        base.Update(dust);

        dust.scale = MathHelper.Lerp(dust.scale, 4f, 0.05f);

        if (dust.frame.X > 9)
            dust.velocity *= 0.9f;
        else
            dust.velocity.Y -= 0.1f;
        dust.alpha += 5;

        if (dust.alpha > 255) dust.active = false;

        return false;
    }
}