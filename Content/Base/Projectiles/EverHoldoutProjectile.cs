namespace Everware.Core.Projectiles;

public abstract class EverHoldoutProjectile : EverProjectile
{
    public Vector2 Offset = Vector2.Zero;
    public float RotationOffset = 0f;
    public float Rotation = 0f;
    public float FrontArmRotationOffset = 0f;
    public float BackArmRotationOffset = 0f;
    public float FrontArmExtension = 0f;
    public float BackArmExtension = 0f;
    public bool TwoHanded = false;
    public override void NetOnSpawn()
    {
        base.NetOnSpawn();
    }
    public override bool PreAI()
    {
        if (Owner.itemTime > 0)
            Projectile.ai[0] = Owner.itemTime;
        else
            Projectile.ai[0]--;

        Owner.heldProj = Projectile.whoAmI;

        Projectile.Center = Owner.Center + Offset;
        Projectile.rotation = Rotation + RotationOffset;

        if (TwoHanded)
            Owner.SetCompositeArmBack(true, StretchAmountFromExtension(BackArmExtension), Rotation + BackArmRotationOffset);
        Owner.SetCompositeArmFront(true, StretchAmountFromExtension(FrontArmExtension), Rotation + FrontArmRotationOffset);

        if (ShouldKill())
        {
            Projectile.Kill();
            return false;
        }

        return base.PreAI();
    }
    public virtual bool ShouldKill()
    {
        return Projectile.ai[0] < -1;
    }
    public static Player.CompositeArmStretchAmount StretchAmountFromExtension(float ext)
    {
        switch (ext)
        {
            case > 0.75f:
                return Player.CompositeArmStretchAmount.Full;
            case > 0.5f:
                return Player.CompositeArmStretchAmount.ThreeQuarters;
            case > 0.25f:
                return Player.CompositeArmStretchAmount.Quarter;
            default:
                return Player.CompositeArmStretchAmount.None;
        }
    }
}
