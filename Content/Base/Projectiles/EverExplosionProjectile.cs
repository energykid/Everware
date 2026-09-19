using Everware.Core.Projectiles;

namespace Everware.Content.Base.Projectiles;

public abstract class EverExplosionProjectile : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Meteor/NPCs/CosmoebaExplosion";

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    public virtual int FrameCount => 5;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.projFrames[Type] = FrameCount;
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.timeLeft = 1000;
        Projectile.tileCollide = false;
        Projectile.hostile = true;
        Projectile.frame = 0;
    }
    public override void AI()
    {
        Projectile.ai[0] += 0.25f;
        Projectile.frame = (int)Projectile.ai[0];

        if (Projectile.frame > FrameCount) Projectile.Kill();

        base.AI();
    }
}