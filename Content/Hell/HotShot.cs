using Everware.Common.Systems;
using Everware.Content.Base.Items;
using Everware.Core.Projectiles;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace Everware.Content.Hell;

public class HotShot : EverWeaponItem
{
    public override string Texture => "Everware/Assets/Textures/Hell/HotShot";
    public override int? HoldoutType => ModContent.ProjectileType<HotShotHoldout>();
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToBasicWeapon(24, 30, DamageClass.Ranged);
    }
}

public class HotShotHoldout : EverHoldoutProjectile
{
    public override string Texture => "Everware/Assets/Textures/Hell/HotShot";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }

    float RotLerpFactor = 1f;
    public override void AI()
    {
        Effects = Owner.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
        Origin = Owner.direction == -1 ? new Vector2(8, 10) : new Vector2(8, 18);

        RotLerpFactor = MathHelper.Lerp(RotLerpFactor, 0.5f, 0.05f);

        Projectile.ai[1]++;

        if (Projectile.ai[1] == 1)
        {
            Fire();
        }

        Scale = Easing.KeyVector2(Projectile.ai[1], 0, 2, Vector2.One, new Vector2(0.75f, 1.25f), Easing.OutCirc, Scale);

        Scale = Easing.KeyVector2(Projectile.ai[1], 2, 10, new Vector2(0.75f, 1.25f), Vector2.One, Easing.InCirc, Scale);

        if (Projectile.ai[1] <= 3)
        {
            RotationOffset -= MathHelper.ToRadians(Easing.KeyFloat(Projectile.ai[1], 0, 6, 10, 0, Easing.Linear)) * Owner.direction;
        }
        else if (Projectile.ai[1] <= 7)
        {
            RotationOffset += MathHelper.ToRadians(Easing.KeyFloat(Projectile.ai[1], 6, 15, 0, 22, Easing.Linear)) * Owner.direction;

        }
        else
        {
            RotationOffset *= 0.6f;
        }
        Rotation = Rotation.AngleLerp(Owner.AngleTo(NetworkOwner.MousePosition), RotLerpFactor);

        base.AI();
    }

    public void Fire()
    {
        SoundEngine.PlaySound(SoundID.Item70.WithPitchOffset(0.5f), Projectile.Center);

        for (int i = 0; i < 6; i++)
        {
            Vector2 vel = Projectile.DirectionTo(NetworkOwner.MousePosition).RotatedByRandom(MathHelper.PiOver4 / 3f) * 40;
            Projectile.NewProjectile(new EntitySource_Parent(Projectile, "Hot Shot"), Projectile.Center + vel * 1.5f, vel, ModContent.ProjectileType<HotShotSlag>(), Projectile.damage, 5f, Projectile.owner, ai1: Math.Sign(NetworkOwner.MousePosition.X - Owner.Center.X));
        }
    }
}

public class HotShotSlag : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Hell/HotShotSlag";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.friendly = true;
        Projectile.knockBack = 2;
        Projectile.tileCollide = true;
        Projectile.DamageType = DamageClass.Ranged;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity = oldVelocity * 0.7f;
        if (ShouldKill == -1)
            ShouldKill = 10;
        return false;
    }

    int Timer = 0;
    int ShouldKill = -1;
    public override void AI()
    {
        if (ShouldKill >= 0)
        {
            ShouldKill--;
            Projectile.velocity *= 0.6f;
            if (ShouldKill == 0) Projectile.Kill();
        }
        if (Projectile.ai[2] == 0)
        {
            Projectile.ai[2] = Main.rand.NextFloat(5f, 10f);
        }
        base.AI();
        Projectile.rotation = Vector2.Zero.AngleTo(Projectile.velocity);
        Timer++;
        if (Timer < 10)
            Projectile.velocity *= 0.9f;
        else
            Projectile.velocity *= 1.05f;
        Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], Projectile.ai[2], 0.1f);
        if (Math.Abs(Projectile.velocity.X) > 10 || Projectile.velocity.Y < 0)
            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(Projectile.ai[0]) * Projectile.ai[1]);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var asset = Assets.Textures.Hell.HotShotSlag.Asset;

        PixelRendering.DrawPixelatedSprite(asset.Value, Projectile.Center - Main.screenPosition, asset.Frame(), Color.White, Projectile.rotation, new Vector2(32, 6), new Vector2(Projectile.velocity.Length() / asset.Width() * 3f, 1f));

        return false;
    }
}

public class HotShotLargeProjectile : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Hell/HotShotLargeProjectile";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.friendly = true;
        Projectile.knockBack = 2;
        Projectile.tileCollide = true;
        Projectile.DamageType = DamageClass.Ranged;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (oldVelocity.X != Projectile.velocity.X)
            Projectile.velocity.X = -oldVelocity.X * 0.5f;
        return false;
    }
}