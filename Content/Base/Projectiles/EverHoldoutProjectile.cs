using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ID;

namespace Everware.Core.Projectiles;

public abstract class EverHoldoutProjectile : EverProjectile
{
    public int AmmoType = AmmoID.None;
    public Vector2 Offset = Vector2.Zero;
    public float RotationOffset = 0f;
    public float Rotation = 0f;
    public float FrontArmRotationOffset = 0f;
    public float BackArmRotationOffset = 0f;
    public float FrontArmExtension = 0f;
    public float BackArmExtension = 0f;
    public bool TwoHanded = false;
    public bool FrontHanded = true;
    public Rectangle Frame = Rectangle.Empty;
    public Vector2 Origin = Vector2.Zero;
    public Vector2 Scale = Vector2.One;
    public SpriteEffects Effects = SpriteEffects.None;
    public bool Persist = false;
    public bool AutoDirection = true;
    public int HitFrames = 0;
    public bool HasMouseBeenReleased = false;
    public bool Started = false;
    public virtual Asset<Texture2D> Asset { get => ModContent.Request<Texture2D>(Texture); }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.penetrate = -1;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 2;
    }
    public override void NetOnSpawn()
    {
        if (Asset != null)
            Frame = Asset.Frame();
    }
    public override bool? CanDamage()
    {
        return (base.CanDamage() == true || base.CanDamage() == null) && HitFrames > 0;
    }
    public override void AI()
    {
        if (!NetworkOwner.MouseDown && Started == true) HasMouseBeenReleased = true;
        Started = true;

        HitFrames--;

        Projectile.timeLeft = 10;

        if (Owner.ItemAnimationActive)
            Projectile.ai[0] = Owner.itemTime;
        else
        {
            Projectile.ai[0]--;

            if (ShouldKill())
            {
                Projectile.Kill();
            }
        }

        Owner.heldProj = Projectile.whoAmI;

        Projectile.Center = Owner.Center + Offset;
        Projectile.rotation = Rotation + RotationOffset;

        if (AutoDirection)
            Owner.direction = Math.Sign(new Vector2(1, 0).RotatedBy(Rotation).X);

        Owner.SetCompositeArmBack(false, Player.CompositeArmStretchAmount.None, 0f);
        Owner.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.None, 0f);

        if (TwoHanded || !FrontHanded)
            Owner.SetCompositeArmBack(true, StretchAmountFromExtension(BackArmExtension), Rotation + BackArmRotationOffset - MathHelper.ToRadians(90f));
        if (TwoHanded || FrontHanded)
            Owner.SetCompositeArmFront(true, StretchAmountFromExtension(FrontArmExtension), Rotation + FrontArmRotationOffset - MathHelper.ToRadians(90f));
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Asset != null)
            Main.EntitySpriteDraw(Asset.Value, Owner.Center + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, Frame, lightColor, Projectile.rotation, Origin, Scale, Effects);

        return false;
    }
    public virtual void UseAmmo(Item item, Player owner)
    {
        if (AmmoType != AmmoID.None)
        {
            if (owner.HasAmmo(item)) owner.ChooseAmmo(item).stack--;
        }
    }
    public virtual bool ShouldKill()
    {
        return Projectile.ai[0] < -2 && !Persist;
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
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        base.ModifyHitNPC(target, ref modifiers);
        modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Owner.Center.X);
    }
}
