using Everware.Core.Projectiles;
using System;

namespace Everware.Content.Gallery.Snapdragon;

public class SnapdragonFrostBreath : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Misc/SinglePixel";
    public override int? TrailSeparation => 10;
    public bool Hit = false;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.ai[0] = 0f;
        Projectile.width = Projectile.height = 128;
        Projectile.hostile = true;
        Projectile.damage = 40;
        Projectile.knockBack = 3;
        Projectile.timeLeft = 40;
        Projectile.ai[2] = Main.rand.NextFloat(-0.3f, 0.3f);
        Projectile.tileCollide = true;
        Projectile.rotation = Main.rand.NextFloat(3f);
    }
    public override void AI()
    {
        Projectile.rotation += Projectile.ai[2];
        if (Projectile.ai[0] < 3) Projectile.velocity *= 1.15f;
        Projectile.velocity *= 0.99f;
        Projectile.scale = MathHelper.Lerp(Projectile.scale, 2f, 0.05f);
        Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 9.9f, 0.05f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Asset<Texture2D> MainTexture = Assets.Textures.Gallery.Snapdragon.SnapdragonFrostBreath.Asset;

        Rectangle fr = MainTexture.Frame(1, 10, frameY: (int)Math.Floor(Projectile.ai[0]));

        Color c = Color.Lerp(Color.White, Color.Blue, Projectile.ai[0] / 10f);

        for (float i = 0; i < Projectile.oldPos.Length; i++)
        {
            Main.EntitySpriteDraw(MainTexture.Value, Projectile.oldPos[(int)i] + (Projectile.Size / 2) - Main.screenPosition, fr, new Color(0.0f, 0.005f, 0.06f, 0.0f), Projectile.rotation, fr.Size() / 2f, MathHelper.Lerp(Projectile.scale * 1.1f, 0f, i / 10f), SpriteEffects.None);
        }

        Main.EntitySpriteDraw(MainTexture.Value, Projectile.Center - Main.screenPosition, fr, c.MultiplyRGBA(new Color(0.5f, 0.65f, 1f, 0f)), Projectile.rotation, fr.Size() / 2f, Projectile.scale, SpriteEffects.None);

        Main.EntitySpriteDraw(MainTexture.Value, Projectile.Center - Main.screenPosition, fr, c.MultiplyRGBA(new Color(0.4f, 0.65f, 1f, 0f)), Projectile.rotation, fr.Size() / 2f, Projectile.scale, SpriteEffects.None);

        return false;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        float length = Math.Abs(Projectile.timeLeft - 40f);

        Vector2 pos = Projectile.Center + new Vector2(Main.rand.NextFloat(-20, 20), Main.rand.NextFloat(-20, 20));

        for (int i = 0; i < 75; i++)
        {
            Vector2 v = Projectile.velocity;
            v.Normalize();
            v *= 10;
            pos += v;
            Point p = (pos / 16).ToPoint();
            if (p.X < 100 || p.X > Main.maxTilesX - 100) break;
            if (p.Y < 100 || p.Y > Main.maxTilesY - 100) break;
            if (Main.tile[(pos / 16).ToPoint()].HasTile && Main.tileSolid[Main.tile[(pos / 16).ToPoint()].TileType]) break;
        }

        Vector2 v1 = Projectile.velocity;
        v1.Normalize();
        v1 *= MathHelper.Clamp(length * 10, 70, 100);

        if (!Hit && Main.tile[(pos / 16).ToPoint()].HasTile && Main.tileSolid[Main.tile[(pos / 16).ToPoint()].TileType])
        {
            if (Main.rand.NextBool(3))
            {

                float p1 = MathHelper.PiOver2;
                float p2 = -MathHelper.PiOver2;
                if (oldVelocity.X < 0)
                {
                    p1 = -MathHelper.PiOver2;
                    p2 = MathHelper.PiOver2;
                }
                Vector2 v = v1 * (length / 5f);
                SnapdragonIceSpikeSystem.AllTriangles.Add(new SnapdragonIceSpikeSystem.IceTriangle(pos, -v, (v1 * 0.5f).RotatedBy(p1), (v1 * 0.5f).RotatedBy(p2)));

            }
            Hit = true;
        }

        Projectile.velocity = oldVelocity * 0.8f;

        return false;
    }
}
