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
    public override void AI()
    {
        base.AI();

    }
    public override bool PreDraw(ref Color lightColor)
    {
        return true;
    }
}