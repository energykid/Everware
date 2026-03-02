using Everware.Content.Base.Items;
using Everware.Core.Projectiles;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace Everware.Content.Hell;

public class Pyrocleavers : EverWeaponItem
{
    public override string Texture => "Everware/Assets/Textures/Hell/Pyrocleavers";
    public override int? HoldoutType => null;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToBasicWeapon(20, 25, DamageClass.Melee);
        Item.shoot = ModContent.ProjectileType<PyrocleaverProjectile>();
        Item.shootSpeed = 8f;
        Item.width = Item.height = 32;
        Item.autoReuse = true;
        Item.UseSound = Assets.Sounds.Gear.Weapon.PyrocleaversThrow.Asset with { PitchVariance = 0.4f };
    }
    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        BaseThrowingUseAnimation(player);
    }
}

public class PyrocleaverProjectile : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Hell/Pyrocleavers";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.width = Projectile.height = 44;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.extraUpdates = 4;
    }
    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.3f, 0f));
        if (Projectile.ai[1] == 0 && WorldGen.SolidOrSlopedTile(Main.tile[(Projectile.Center / 16f).ToPoint()]))
        {
            SoundStyle style = Assets.Sounds.Gear.Weapon.PyrocleaversLavaBubble.Asset;
            style.PitchRange = (-0.5f, 0f);
            style.MaxInstances = 20;

            SoundEngine.PlaySound(style, Projectile.Center);

            Projectile.velocity = Vector2.Zero;
            Projectile.ai[1] = 1;

            Projectile.Center = Projectile.Center.Grounded();
        }
        Projectile.hide = true;
        if (Projectile.ai[1] == 1)
        {
            SlashOpacity *= 0.2f;
            Projectile.extraUpdates = 0;
            if (Projectile.ai[2] % 5 == 2)
            {
                int Reach = (int)Projectile.ai[2] / 5;

                int tileXCenter = ((int)Math.Round(Projectile.Center.X / 16) * 16) - 8;

                if (Main.myPlayer == Projectile.owner)
                    Projectile.NewProjectileDirect(new EntitySource_Parent(Projectile, "Pyrocleaver lava pool"), new Vector2(tileXCenter - (Reach * 16), Projectile.Top.Grounded().Y), Vector2.Zero,
                        ModContent.ProjectileType<FloorIsLava>(), Projectile.damage / 2, Projectile.knockBack / 3, Projectile.owner, -(Reach * 3));
                if (Reach != 0)
                {
                    Projectile.NewProjectileDirect(new EntitySource_Parent(Projectile, "Pyrocleaver lava pool"), new Vector2(tileXCenter + (Reach * 16), Projectile.Top.Grounded().Y), Vector2.Zero,
                        ModContent.ProjectileType<FloorIsLava>(), Projectile.damage / 2, Projectile.knockBack / 3, Projectile.owner, -(Reach * 3));
                }
            }

            Projectile.velocity.Y += 0.1f;
            Projectile.ai[2]++;
            if (Projectile.ai[2] > 20)
            {
                Projectile.Kill();
            }
        }
        else
        {
            effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Projectile.ai[0]++;
            if (Projectile.ai[0] > 20)
            {
                if (Projectile.velocity.Y > 2 && Projectile.ai[0] > 40)
                    SlashOpacity = MathHelper.Lerp(SlashOpacity, 1f, 0.1f);
                Projectile.extraUpdates = 1;
                Projectile.velocity.X *= 0.99f;
                Projectile.velocity.Y += 0.01f * (Projectile.ai[0] - 20);
            }
            Projectile.rotation += (Math.Abs(MathHelper.ToRadians(Projectile.velocity.X * 1.2f)) + Math.Abs(MathHelper.ToRadians(Projectile.velocity.Y * 1.2f))) * Math.Sign(Projectile.velocity.X);
        }
    }
    public SpriteEffects effects = SpriteEffects.None;
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return base.OnTileCollide(oldVelocity);
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
    }
    float SlashOpacity = 0f;
    public override bool PreDraw(ref Color lightColor)
    {
        var asset = Assets.Textures.Hell.Pyrocleavers.Asset;
        var asset2 = Assets.Textures.Hell.Pyrocleavers_Glow.Asset;

        var asset3 = Assets.Textures.Misc.Slash.Asset;
        var asset4 = Assets.Textures.Misc.LensFlash.Asset;

        var frame = asset.Frame();

        var slashOrigin = new Vector2(effects == SpriteEffects.None ? -20 : asset3.Size().X + 20, asset3.Size().Y / 2f);

        DrawingUtils.DrawTrailBehind(Projectile, Color.Orange.MultiplyRGBA(new(1f, 1f, 1f, 0.5f)), Color.Maroon.MultiplyRGBA(new(0.1f, 0.1f, 0.1f, 0f)), Vector2.Zero, customTexture: Assets.Textures.Hell.Pyrocleavers_Glow.Asset);

        Main.EntitySpriteDraw(asset.Value, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2f, 1f, effects);
        Main.EntitySpriteDraw(asset2.Value, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() / 2f, 1f, effects);

        Main.EntitySpriteDraw(asset3.Value, Projectile.Center - Main.screenPosition, asset3.Frame(), Color.Maroon.MultiplyRGBA(new(SlashOpacity, SlashOpacity, SlashOpacity, 0.2f * SlashOpacity)), Projectile.rotation, slashOrigin, 0.35f, effects);
        Main.EntitySpriteDraw(asset3.Value, Projectile.Center - Main.screenPosition, asset3.Frame(), Color.OrangeRed.MultiplyRGBA(new(SlashOpacity, SlashOpacity, SlashOpacity, 0.2f * SlashOpacity)), Projectile.rotation, slashOrigin, 0.425f, effects);
        Main.EntitySpriteDraw(asset3.Value, Projectile.Center - Main.screenPosition, asset3.Frame(), Color.Orange.MultiplyRGBA(new(SlashOpacity, SlashOpacity, SlashOpacity, 0.2f * SlashOpacity)), Projectile.rotation, slashOrigin, 0.5f, effects);

        Main.EntitySpriteDraw(asset4.Value, Projectile.Center - Main.screenPosition + new Vector2(effects == SpriteEffects.None ? 30 : -30, 0).RotatedBy(Projectile.rotation), asset4.Frame(), Color.Orange.MultiplyRGBA(new(SlashOpacity, SlashOpacity, SlashOpacity, 0.2f * SlashOpacity)), 0f, asset4.Size() / 2f, 0.3f, SpriteEffects.None);

        return false;
    }
}

public class FloorIsLava : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Hell/FloorIsLava";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.width = Projectile.height = 16;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 2000;
    }
    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.3f, 0f) * MathHelper.Clamp((Projectile.ai[0] / 5), 0f, 1f));

        Projectile.position = Projectile.position.Grounded() + new Vector2(0, -16);
        if (Main.tile[((Projectile.position / 16) + new Vector2(0, 1)).ToPoint()].IsHalfBlock)
        {
            Projectile.position.Y += 8;
        }
        Projectile.position.Y = (float)Math.Round(Projectile.position.Y / 8) * 8;
        base.AI();
        Projectile.ai[1]++;
        if (Projectile.ai[1] < 200)
        {
            Projectile.ai[0] += 0.45f;
            if (Projectile.ai[0] > 3) Projectile.ai[0] -= 0.25f;
            if (Projectile.ai[0] > 7)
            {
                Projectile.ai[0] -= 4;
            }
        }
        else
        {
            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], -1, 0.15f);
            if (Projectile.ai[0] < 0) Projectile.Kill();
        }
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return false;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var asset = Assets.Textures.Hell.FloorIsLava.Asset;

        int tileFrameX = 0;

        int i = (int)Projectile.position.X / 16;
        int j = (int)Projectile.position.Y / 8;

        if (IsFloorLavaAt(i - 1, j))
        {
            tileFrameX = 3;
            if (IsFloorLavaAt(i + 1, j))
            {
                tileFrameX = 2;
            }
        }
        else
        {
            if (IsFloorLavaAt(i + 1, j))
            {
                tileFrameX = 1;
            }
        }

        var frame = asset.Frame(4, 8, tileFrameX, (int)MathHelper.Max((float)Math.Floor(Projectile.ai[0]), 0));

        SpriteEffects eff = SpriteEffects.None;
        if (i % 2 == 0 && tileFrameX == 2) eff = SpriteEffects.FlipHorizontally;

        Main.EntitySpriteDraw(asset.Value, Projectile.Center - Main.screenPosition + new Vector2(0, 16) + new Vector2(0, 3), frame, Color.White, 0f, frame.Size() / 2f, 1f, eff);

        return false;
    }
    public bool IsFloorLavaAt(int i, int j)
    {
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.Distance(new Vector2((i * 16) + 8, (j * 8) + 8)) < 2 && projectile.ai[0] > 3)
            {
                return true;
            }
        }
        return false;
    }
}
