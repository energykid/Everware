using Everware.Content.Base.Items;
using Everware.Core.Projectiles;

namespace Everware.Content.PreHardmode.Kiln.Gear;

public class Kilnpost : EverWeaponItem
{
    public override int? HoldoutType => ModContent.ProjectileType<KilnpostHoldout>();
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToBasicWeapon(10, 20, DamageClass.Melee);
    }
}
public class KilnpostHoldout : EverHoldoutProjectile
{
    public override string Texture => "Everware/Content/PreHardmode/Kiln/Gear/Kilnpost";
    public override bool TwoHanded => true;
    public override float RotationOffset => MathHelper.ToRadians(50f);
    public override Vector2 HoldoutOrigin => new Vector2();
    public override void AI()
    {
        base.AI();

        float outset = 20f;
        float rotation = NetworkOwner.MousePosition.AngleFrom(Owner.Center);

        HitboxOutset = (int)outset;
        Projectile.rotation = rotation;
        Projectile.Center = Owner.Center + new Vector2(outset, 0).RotatedBy(Projectile.rotation);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}