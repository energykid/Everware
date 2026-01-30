using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Everware.Content.Base.Dusts;

public abstract class EverDust : ModDust
{
    public override bool Update(Dust dust)
    {
        dust.position += dust.velocity;
        return false;
    }

    public override bool PreDraw(Dust dust)
    {
        float a = 1f - ((float)dust.alpha / 255f);
        Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture).Value, dust.position - Main.screenPosition, dust.frame, Color.White.MultiplyRGBA(new(a, a, a, a)).MultiplyRGBA(Lighting.GetColor(new((int)dust.position.X / 16, (int)dust.position.Y / 16))), dust.rotation, dust.frame.Size() / 2f, dust.scale, SpriteEffects.None);
        return false;
    }
}
