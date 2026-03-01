using Everware.Content.Base.Items;
using Everware.Core.Projectiles;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace Everware.Content.Hell;

public class BloodRubyCharm : EverItem
{
    public static int DashDamage => 60;
    public override string Texture => "Everware/Assets/Textures/Hell/BloodRubyCharm";

    public override int DuplicationAmount => 1;

    public override void SetDefaults()
    {
        Item.DefaultToAccessory();
        Item.width = 30;
        Item.height = 32;
        Item.defense = 2;
        Item.rare = 5;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<BloodRubyDashPlayer>().BloodRubyEffect = true;
    }
}
public class BloodRubyExplosion : EverProjectile
{
    int Direction = 1;
    public override string Texture => "Everware/Assets/Textures/Hell/BloodRubyExplosion";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.width = 78;
        Projectile.height = 44;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public Vector2 Scale = new Vector2(3f, 1f);
    public override void AI()
    {
        float intensity = MathHelper.Lerp(2f, 0f, Projectile.ai[1] / 6f);
        Lighting.AddLight(Projectile.Center, new Vector3(intensity * 0.4f, intensity * 0.3f, 0f));
        Scale = Easing.KeyVector2(Projectile.ai[1], 0f, 1f, new Vector2(0.5f, 2f), new Vector2(3f, 1f), Easing.OutCirc);
        if (Projectile.ai[1] > 1f)
            Scale = Easing.KeyVector2(Projectile.ai[1], 1f, 4f, new Vector2(3f, 1f), new Vector2(1f, 1f), Easing.OutCirc);
        Projectile.Center = Main.player[Projectile.owner].Center + new Vector2(-60 * Projectile.ai[0], 0);
        base.AI();
        Projectile.ai[1] += 0.333f;
        if (Projectile.ai[1] > 6) Projectile.Kill();
        if (Projectile.ai[1] > 1.5) Projectile.damage = 0;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var asset = Assets.Textures.Hell.BloodRubyExplosion.Asset;
        var frame = asset.Frame(1, 6, 0, (int)Math.Floor(Projectile.ai[1]));

        Main.EntitySpriteDraw(asset.Value, Projectile.Center - Main.screenPosition + new Vector2(Projectile.ai[0] * 50, Main.player[Projectile.owner].gfxOffY), frame, new Color(1f, 1f, 1f, 0f), 0f, new(Projectile.ai[0] == 1 ? 78 : 0, 24), Scale, Projectile.ai[0] == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

        return false;
    }
}
public class BloodRubyDashPlayer : ModPlayer
{
    public int DashDirection = 0;

    public const int DashCooldown = 50;
    public const int DashDuration = 35;

    public float LerpAmount = 1f;

    public const float DashVelocity = 12f;

    public int DashDir = 0;

    public bool BloodRubyEffect;
    public int DashDelay = 0;
    public int DashTimer = 0;

    public override void ResetEffects()
    {
        BloodRubyEffect = false;

        if (DashTimer <= 0)
        {
            if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[2] < 15 && Player.doubleTapCardinalTimer[3] == 0)
            {
                DashDir = 1;
            }
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[3] < 15 && Player.doubleTapCardinalTimer[2] == 0)
            {
                DashDir = -1;
            }
        }
    }

    public override void PreUpdateMovement()
    {
        LerpAmount = MathHelper.Lerp(LerpAmount, 1f, 0.2f);

        if (CanUseDash() && DashDir != 0 && DashDelay == 0)
        {
            Vector2 newVelocity = Player.velocity;

            switch (DashDir)
            {
                case -1 when Player.velocity.X > -40:
                case 1 when Player.velocity.X < 40:
                    {
                        newVelocity.X = DashDir * 40;
                        break;
                    }
                default:
                    return;
            }

            // start our dash
            DashDelay = DashCooldown;
            DashTimer = DashDuration;
            Player.velocity = newVelocity;

            Explosion();

            LerpAmount = 0.7f;

            DashDir = 0;
        }

        if (DashDelay > 0)
            DashDelay--;

        if (DashTimer > 0)
        {
            if (DashTimer > DashDuration - 5)
            {
                Player.velocity.Y = MathHelper.Min(Player.velocity.Y, 0.5f);
            }
            if (DashTimer > DashDuration - 20)
            {

            }
            Player.eocDash = DashTimer;
            Player.armorEffectDrawShadowEOCShield = true;

            DashTimer--;
        }

        Player.velocity.X *= LerpAmount;
    }

    private bool CanUseDash()
    {
        return BloodRubyEffect
            && Player.dashType == DashID.None
            && !Player.setSolar
            && !Player.mount.Active;
    }

    public void Explosion()
    {
        SoundEngine.PlaySound(Assets.Sounds.Gear.Accessory.BloodRubyDash.Asset, Player.Center);

        if (Main.myPlayer == Player.whoAmI)
        {
            Projectile proj = Projectile.NewProjectileDirect(new EntitySource_Parent(Player, "Devil's Ruby"), Player.Center + new Vector2(-30 * DashDir, 0), Vector2.Zero, ModContent.ProjectileType<BloodRubyExplosion>(), BloodRubyCharm.DashDamage, 2f, Player.whoAmI, DashDir, ai2: Player.whoAmI);
        }
    }
}