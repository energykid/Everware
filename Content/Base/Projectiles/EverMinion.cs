using Everware.Core.Projectiles;

namespace Everware.Content.Base.Projectiles;

public abstract class EverMinion : EverProjectile
{
    int target = 0;
    public virtual int BuffType => 0;
    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.minionSlots = 1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
    }
    public override void AI()
    {
        if (!Owner.HasBuff(BuffType))
        {
            Projectile.Kill();
        }
    }
}
