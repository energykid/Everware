using Everware.Common.Systems;
using Everware.Content.Base.Items;
using Everware.Core.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace Everware.Content.PreHardmode.Quarry.Gear;

public class Rebuker : EverWeaponItem
{
    public static readonly SoundStyle LoadSound = new SoundStyle("Everware/Sounds/Gear/Weapon/RebukerLoad") with { PitchRange = (-0.2f, 0.2f) };
    public static readonly SoundStyle FireSound = new SoundStyle("Everware/Sounds/Gear/Weapon/RebukerFire") with { PitchRange = (-0.2f, 0.2f) };
    public override int? HoldoutType => ModContent.ProjectileType<RebukerHoldout>();
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToBasicWeapon(9, 65, DamageClass.Ranged);
        Item.knockBack = 1f;
    }
}

public class RebukerHoldout : EverHoldoutProjectile
{
    public int FrameY = 0;
    public float RotLerpFactor = 0.6f;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Origin = new Vector2(8, 18);
    }
    public override void NetOnSpawn()
    {
        Rotation = Owner.AngleTo(NetworkOwner.MousePosition);
    }
    public override bool? CanDamage()
    {
        return false;
    }
    public override void AI()
    {
        TwoHanded = true;
        Effects = Owner.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
        Origin = Owner.direction == -1 ? new Vector2(8, 10) : new Vector2(8, 18);

        Frame = new Rectangle(0, 28 * FrameY, 52, 28);

        RotLerpFactor = MathHelper.Lerp(RotLerpFactor, 0.7f, 0.05f);

        Rotation = Rotation.AngleLerp(Owner.AngleTo(NetworkOwner.MousePosition), RotLerpFactor);

        Projectile.ai[1]++;

        int ShootTime = Owner.itemAnimationMax / 2;
        int LoadTime = 1;

        if (Projectile.ai[1] == ShootTime)
        {
            ShootRebarBolt();
            RotLerpFactor = 0f;
        }
        if (Projectile.ai[1] == LoadTime)
        {
            LoadRebarBolt();
        }

        if (Projectile.ai[1] < ShootTime - 10)
        {
            Offset = Vector2.Lerp(Offset, new Vector2(0, 2), 0.05f);

            if (Projectile.ai[1] < ShootTime - 20) FrameY = 1;
            else FrameY = 0;

            RotationOffset = Owner.direction * MathHelper.ToRadians(50f);

            float PointInAnimation = Projectile.ai[1] / ShootTime;

            FrontArmExtension = MathHelper.Clamp((PointInAnimation * 2f), 0.5f, 1f);
        }
        else
        {
            // Shoot animation
            if (Projectile.ai[1] >= ShootTime)
            {
                if (Projectile.ai[1] < ShootTime + 7)
                    FrontArmExtension *= 0.4f;
                else
                    FrontArmExtension = MathHelper.Lerp(FrontArmExtension, 1f, 0.1f);
                RotationOffset *= 0.9f;
                if (Projectile.ai[1] == ShootTime)
                {
                    RotationOffset = Owner.direction * MathHelper.ToRadians(-20f);
                }
                FrontArmRotationOffset = RotationOffset;
            }
            else
            {
                RotationOffset *= 0.5f;
            }
            if (Projectile.ai[1] > ShootTime)
            {
                FrontArmRotationOffset = RotationOffset * 2f;
                Offset = Vector2.Lerp(Offset, new Vector2(-Owner.direction * 10, -2), 0.5f);
                FrameY = 2;
            }
            if (Projectile.ai[1] > ShootTime + 5)
            {
                Offset = Vector2.Lerp(Offset, new Vector2(-Owner.direction * 8, 1), 0.3f);
                FrameY = 3;
            }
            if (Projectile.ai[1] > ShootTime + 10)
            {
                Offset = Vector2.Lerp(Offset, Vector2.Zero, 0.2f);
                FrameY = 4;
            }
        }
        BackArmRotationOffset = RotationOffset;
        base.AI();
    }
    public void ShootRebarBolt()
    {
        SoundEngine.PlaySound(Rebuker.FireSound, Projectile.Center);
        Vector2 SpawnLocation = Owner.MountedCenter + new Vector2(40, 0).RotatedBy(Projectile.rotation);
        Projectile proj = Projectile.NewProjectileDirect(new EntitySource_ItemUse(Owner, Owner.HeldItem), SpawnLocation, new Vector2(3, 0).RotatedBy(Projectile.rotation), ModContent.ProjectileType<RebukerBolt>(), 12, 3f, Projectile.owner);
    }
    public void LoadRebarBolt()
    {
        SoundEngine.PlaySound(Rebuker.LoadSound, Projectile.Center);
    }
}

public class RebukerBolt : EverProjectile
{
    bool HasHit = false;
    int HitTimer = 0;
    int NumberOfHits = 0;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.penetrate = -1;
        Projectile.timeLeft = 1000;
        Projectile.friendly = true;
    }
    public override void NetOnSpawn()
    {
        Projectile.hide = true;
        Projectile.extraUpdates = 60;
        Projectile.ai[0] = Main.rand.NextFloat(100f);
        Projectile.ai[1] = 1f;
        Projectile.ai[2] = 0.2f;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);
        if (Projectile.damage > 4) Projectile.damage -= 3;
    }
    public override bool? CanDamage()
    {
        return base.CanDamage();
    }
    public override void AI()
    {
        base.AI();
        if (!HasHit)
        {
            Projectile.ai[1] *= 0.95f;
            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 1.4f, 0.05f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.UnusedWhiteBluePurple, new Vector2((float)Math.Sin(Projectile.ai[0] / 5) * 2.5f * Projectile.ai[1], 0).RotatedBy(Projectile.velocity.AngleFrom(Vector2.Zero) + MathHelper.PiOver2), Scale: Projectile.ai[2] * 0.8f);
            dust.noGravity = true;
            dust.noLight = true;
            Dust.NewDustPerfect(Projectile.Center, DustID.SpelunkerGlowstickSparkle, new Vector2((float)Math.Sin(Projectile.ai[0] / 5.5f) * 2.2f * Projectile.ai[1], 0).RotatedBy(Projectile.velocity.AngleFrom(Vector2.Zero) + MathHelper.PiOver2), Scale: Projectile.ai[2]);
            Dust.NewDustPerfect(Projectile.Center, DustID.SpelunkerGlowstickSparkle, new Vector2((float)Math.Sin(Projectile.ai[0] / 11.5f) * 1.2f, 0).RotatedBy(Projectile.velocity.AngleFrom(Vector2.Zero) + MathHelper.PiOver2), Scale: Projectile.ai[2] * 0.5f);
            Projectile.rotation = Vector2.Zero.AngleTo(Projectile.velocity);
        }
        else
        {
            if (Projectile.extraUpdates == 0)
            {
                HitTimer++;
                Projectile.extraUpdates = 0;
                Projectile.damage = 0;
                if (HitTimer > 50) Projectile.alpha += 10;
                if (Projectile.alpha > 255) Projectile.Kill();
            }
        }

        Projectile.ai[0]++;
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        ScreenEffects.AddScreenShake(Projectile.Center, 4f, 0.5f);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Tink.WithPitchOffset(-0.5f), Projectile.Center);
        HasHit = true;
        Projectile.damage = 0;
        Projectile.extraUpdates = 0;
        Projectile.Center += Projectile.velocity * 4f;
        Projectile.velocity = Vector2.Zero;
        Projectile.tileCollide = false;
        return false;
    }
}