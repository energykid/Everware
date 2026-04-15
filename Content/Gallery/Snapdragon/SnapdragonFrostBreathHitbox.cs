using Everware.Core.Projectiles;

namespace Everware.Content.Gallery.Snapdragon;

public class SnapdragonFrostBreathHitbox : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Misc/SinglePixel";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.width = Projectile.height = 250;
        Projectile.hostile = true;
        Projectile.damage = 40;
        Projectile.knockBack = 3;
        Projectile.hide = true;
        Projectile.timeLeft = 15;
        Projectile.tileCollide = true;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
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

        SoundStyle asset = Assets.Sounds.NPC.Snapdragon_Assemble.Asset;
        asset.MaxInstances = 20;
        if (Main.rand.NextBool(2)) SoundEngine.PlaySound(asset.WithPitchVariance(0.6f).WithPitchOffset(0.4f).WithVolumeScale(0.3f), pos);

        if (Main.tile[(pos / 16).ToPoint()].HasTile && Main.tileSolid[Main.tile[(pos / 16).ToPoint()].TileType])
        {
            float p1 = MathHelper.PiOver2;
            float p2 = -MathHelper.PiOver2;
            if (oldVelocity.X < 0)
            {
                p1 = -MathHelper.PiOver2;
                p2 = MathHelper.PiOver2;
            }
            SnapdragonIceSpikeSystem.AllTriangles.Add(new SnapdragonIceSpikeSystem.IceTriangle(pos, -Projectile.velocity * 1.5f, (Projectile.velocity * 0.2f).RotatedBy(p1), (Projectile.velocity * 0.2f).RotatedBy(p2)));
        }

        Projectile.Kill();

        return false;
    }
}
