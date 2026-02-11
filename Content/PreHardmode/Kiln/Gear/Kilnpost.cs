using Everware.Content.Base.Items;
using Everware.Core.Projectiles;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Kiln.Gear;

public class Kilnpost : EverWeaponItem
{
    public static readonly SoundStyle ThrustSound = new SoundStyle("Everware/Sounds/Gear/Weapon/KilnpostThrust") with { PitchRange = (-0.2f, 0.2f) };
    public static readonly SoundStyle BreakawaySound = new SoundStyle("Everware/Sounds/Gear/Weapon/KilnpostBreakaway") with { PitchRange = (-0.2f, 0.2f) };
    public override int? HoldoutType => ModContent.ProjectileType<KilnpostHoldout>();
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToBasicWeapon(10, 40, DamageClass.Melee);
    }
}
public class KilnpostHoldout : EverHoldoutProjectile
{
    string State = "Thrust";
    float Spin = 0f;
    public override string Texture => "Everware/Content/PreHardmode/Kiln/Gear/Kilnpost";
    public override void AI()
    {
        TwoHanded = false;
        if (Owner.itemAnimation == Owner.itemAnimationMax / 3 || Owner.ItemAnimationJustStarted)
        {
            Projectile.ai[1] = 0f;
            Projectile.ai[2] = 70f;
            Rotation = Owner.AngleTo(NetworkOwner.MousePosition);
            State = (State == "Spin") ? "Thrust" : "Spin";
            if (State == "Thrust")
            {
                FrontArmExtension = 1f;
                SoundEngine.PlaySound(Kilnpost.ThrustSound, Owner.Center);
            }
            else
                BackArmExtension = 0.8f;
        }
        if (State == "Spin")
            SpinMotion();
        if (State == "Thrust")
            ThrustMotion();
        base.AI();
    }
    public void ThrustMotion()
    {
        Projectile.damage = 10;
        Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 30f, 0.6f);

        RotationOffset = MathHelper.ToRadians(50f);

        FrontArmExtension = MathHelper.Lerp(FrontArmExtension, 0.2f, 0.2f);
        BackArmExtension = MathHelper.Lerp(BackArmExtension, 0f, 0.4f);

        Origin = Asset.Frame().Size() / 2f;
        Offset = new Vector2(Projectile.ai[2], 0).RotatedBy(Rotation);
    }
    public void SpinMotion()
    {
        Projectile.damage = 3;

        Projectile.ai[1]++;

        if (Projectile.ai[1] % 4 == 0 && Projectile.ai[1] < 25)
        {
            SoundEngine.PlaySound(SoundID.Item1.WithPitchOffset(0.8f - (Projectile.ai[1] * 0.05f)).WithVolumeScale(1f - (Projectile.ai[1] * 0.038f)), Owner.Center);
        }

        FrontArmExtension = 0f;
        BackArmExtension = MathHelper.Lerp(BackArmExtension, 0f, 0.05f);

        float spinSpeed = Easing.KeyFloat((float)Owner.itemAnimation, 0f, (float)Owner.itemAnimationMax, 0f, 0.7f, Easing.Linear);

        Origin = Asset.Frame().Size() / 2f;
        Offset = new Vector2(10, 0).RotatedBy(Rotation);
        RotationOffset = MathHelper.ToRadians(135f) + Spin;
        Spin += Owner.direction * spinSpeed;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        return base.PreDraw(ref lightColor);
    }
}