using Everware.Core.Projectiles;
using Everware.Utils;

namespace Everware.Content.Gallery.Snapdragon;

public class SnapdragonIceSpike : EverProjectile
{
    public Vector2 Position1 = Vector2.Zero;
    public Vector2 Position2 = Vector2.Zero;
    public Vector2 Position3 = Vector2.Zero;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.hostile = true;
        Projectile.width = Projectile.height = 200;
        Projectile.aiStyle = -1;
        Projectile.coldDamage = true;
    }
    public void SetTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Position1 = p1;
        Position2 = p2;
        Position3 = p3;
    }
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox = new Rectangle((int)Projectile.Center.X - 200, (int)Projectile.Center.Y - 200, 400, 400);
    }
    public override bool CanHitPlayer(Player target)
    {
        return base.CanHitPlayer(target) && MathUtils.PointInTriangle(target.Center, Position1, Position2, Position3);
    }
    public override void AI()
    {
        base.AI();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}
