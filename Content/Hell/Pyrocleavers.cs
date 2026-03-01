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
        }
        Projectile.hide = true;
        if (Projectile.ai[1] == 1)
        {
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
                Projectile.extraUpdates = 1;
                Projectile.velocity.X *= 0.99f;
                Projectile.velocity.Y += 0.01f * (Projectile.ai[0] - 20);
            }
            Projectile.rotation += MathHelper.ToRadians(Projectile.velocity.X * 3f);
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
    public override bool PreDraw(ref Color lightColor)
    {
        var asset = Assets.Textures.Hell.Pyrocleavers.Asset;
        var asset2 = Assets.Textures.Hell.Pyrocleavers_Glow.Asset;

        var frame = asset.Frame();

        Main.EntitySpriteDraw(asset.Value, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2f, 1f, effects);
        Main.EntitySpriteDraw(asset2.Value, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() / 2f, 1f, effects);

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
        Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.3f, 0f) * (Projectile.ai[0] / 5));

        Projectile.position = Projectile.position.Grounded() + new Vector2(0, -16);
        Projectile.position.Y = (float)Math.Round(Projectile.position.Y / 16) * 16;
        base.AI();
        Projectile.ai[1]++;
        if (Projectile.ai[1] < 200)
        {
            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 4f, 0.15f);
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
        int j = (int)Projectile.position.Y / 16;

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

        var frame = asset.Frame(4, 5, tileFrameX, (int)MathHelper.Max((float)Math.Floor(Projectile.ai[0]), 0));

        Main.EntitySpriteDraw(asset.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Main.tile[i, j + 1].IsHalfBlock ? 24 : 16), frame, Color.White, 0f, frame.Size() / 2f, 1f, SpriteEffects.None);

        return false;
    }
    public bool IsFloorLavaAt(int i, int j)
    {
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.Distance(new Vector2((i * 16) + 8, (j * 16) + 8)) < 2)
            {
                return true;
            }
        }
        return false;
    }
}
