using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.ID;

namespace Everware.Core.Projectiles;

public abstract class EverHoldoutProjectile : EverProjectile
{
    /// <summary>
    ///     Defines which sound plays when PlayUseSound is called.
    /// </summary>
    public virtual SoundStyle UseSound => SoundID.DD2_MonkStaffSwing;

    /// <summary>
    ///     Whether or not the visual fades in with scale.
    /// </summary>
    public virtual bool FadeIn => false;

    /// <summary>
    /// The amount of pixels outwards the center of the hitbox will be from the player.
    /// </summary>
    public int HitboxOutset = 30;

    /// <summary>
    ///     Defines the offset of rotation. This is for if a sprite is oriented in a certain way.
    /// </summary>
    public virtual float RotationOffset => 0f;

    /// <summary>
    ///     Defines the origin of the projectile. Used for handle-related things.
    /// </summary>
    public virtual Vector2 HoldoutOrigin => Vector2.Zero;

    /// <summary>
    ///     Whether the held projectile occupies both hands.
    /// </summary>
    public virtual bool TwoHanded => false;

    /// <summary>
    ///     Defines the length of HoldoutOffset. Set this if you want to define a HoldoutOffset that extends the item outwards by a set distance without hassle.
    /// </summary>
    public virtual float HoldoutLength => 0f;

    /// <summary>
    ///     Defines the offset of position. Usually used in the context of holdout offset.
    /// </summary>
    public virtual Vector2 HoldoutOffset => new Vector2(HoldoutLength, 0).RotatedBy(Projectile.rotation);

    /// <summary>
    ///     Defines the type of projectile the item shoots upon use (such as an arrow or bullet).
    /// </summary>
    public virtual int FireType => ProjectileID.None;

    /// <summary>
    ///     Defines the speed of the projectile the item shoots upon the use (see FireType).
    /// </summary>
    public virtual float FireSpeed => 10f;

    public int AmmoType = 0;

    private float AnimationSpeed = 1f;

    public float AnimationTime = 0f;

    public float HitFrames;

    public float ScaleIncrement;

    public bool Flip = false;

    public bool ShouldPersist = false;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ProjectileID.Sets.TrailingMode[Projectile.type] = 4;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Projectile.damage = 20;
        Projectile.knockBack = 2f;
        Projectile.penetrate = -1;

        Projectile.tileCollide = false;

        Projectile.friendly = true;

        if (!FadeIn)
        {
            ScaleIncrement = 1f;
        }

        Projectile.netImportant = true;

        Projectile.ContinuouslyUpdateDamageStats = true;
    }

    public override bool? CanDamage()
    {
        if (HitFrames > 0)
        {
            return base.CanDamage();
        }

        return false;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        var Size = HitboxOutset;
        var Size2 = Projectile.Size;

        var vec = Projectile.Center + new Vector2(Size, 0).RotatedBy(Projectile.rotation);

        hitbox = new Rectangle((int)vec.X - (int)(Size2.X / 2), (int)vec.Y - (int)(Size2.Y / 2), (int)Size2.X, (int)Size2.Y);
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        var Size = Projectile.Size.Length();

        hitboxCenterFrac = Projectile.Center + new Vector2(Size / 2, 0).RotatedBy(Projectile.rotation);

        return false;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Owner.MountedCenter.X);
    }
    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Owner.Center.X);
    }

    int Direction = 0;

    public override void AI()
    {
        if (Owner != null)
        {
            if (Owner.dead)
                Projectile.Kill();

            AnimationTime += AnimationSpeed;

            Direction = Math.Sign(NetworkOwner.MousePosition.X - Owner.Center.X);
            Owner.direction = Direction;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            if (TwoHanded)
            {
                Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));
            }

            Projectile.Center = Owner.Center;

            Projectile.timeLeft = 100;

            Main.player[Projectile.owner].heldProj = Projectile.whoAmI;

            if (HitFrames >= 0)
            {
                HitFrames--;
            }
            else
            {
                HitFrames = 0;
            }

            if (((!Owner.ItemAnimationActive && !Owner.controlUseItem) || Owner.HeldItem.shoot != Projectile.type) && !ShouldPersist)
            {
                Projectile.Kill();
            }
        }
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        base.SendExtraAI(writer);

        writer.Write(Direction);
        writer.Write(AnimationTime);
        writer.Write(AnimationSpeed);
        writer.Write(HitFrames);
        writer.Write(ScaleIncrement);
        writer.Write(ShouldPersist);
        writer.Write(Projectile.owner);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        base.ReceiveExtraAI(reader);

        Direction = reader.ReadInt32();
        AnimationTime = reader.ReadSingle();
        AnimationSpeed = reader.ReadSingle();
        HitFrames = reader.ReadSingle();
        ScaleIncrement = reader.ReadSingle();
        ShouldPersist = reader.ReadBoolean();
        Projectile.owner = reader.ReadInt32();
    }

    public static void AddHitFrames(Projectile projectile)
    {
        (projectile.ModProjectile as EverHoldoutProjectile).HitFrames = 4;
    }

    public static void AddShortHitFrames(Projectile projectile)
    {
        (projectile.ModProjectile as EverHoldoutProjectile).HitFrames = 2;
    }

    public static void PlayUseSound(Projectile projectile)
    {
        SoundEngine.PlaySound((projectile.ModProjectile as EverHoldoutProjectile).UseSound, (projectile.ModProjectile as EverHoldoutProjectile).Owner.Center);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var tex = ModContent.Request<Texture2D>(Texture);

        if (!Flip)
        {
            Main.EntitySpriteDraw(
                tex.Value,
                Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Owner.gfxOffY),
                tex.Frame(),
                lightColor,
                Projectile.rotation + RotationOffset,
                HoldoutOrigin,
                Projectile.scale * Owner.HeldItem.scale * ScaleIncrement,
                SpriteEffects.None
            );
        }
        else
        {
            Main.EntitySpriteDraw(
                tex.Value,
                Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Owner.gfxOffY),
                tex.Frame(),
                lightColor,
                Projectile.rotation + RotationOffset + MathHelper.PiOver2,
                new Vector2(MathHelper.Lerp(tex.Size().X, tex.Size().X - 1, HoldoutOrigin.X), HoldoutOrigin.Y),
                Projectile.scale * Owner.HeldItem.scale * ScaleIncrement,
                SpriteEffects.FlipHorizontally
            );
        }

        return false;
    }
}
