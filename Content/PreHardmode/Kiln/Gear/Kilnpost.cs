using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Content.PreHardmode.Kiln.Tiles;
using Everware.Content.PreHardmode.Kiln.Visual;
using Everware.Core.Projectiles;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
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
        Item.DefaultToBasicWeapon(8, 40, DamageClass.Melee);
        Item.knockBack = 1f;
    }
    public override void AddRecipes()
    {
        Recipe recipe = Recipe.Create(Type);
        recipe.AddIngredient(ModContent.ItemType<KilnBrick>(), 20);
        recipe.AddIngredient(ModContent.ItemType<Kilnstone>(), 5);
        recipe.AddTile(ModContent.TileType<ForgingKiln>());
        recipe.Register();
    }
}
public class KilnpostHoldout : EverHoldoutProjectile
{
    string State = "Thrust";
    float Spin = 0f;
    float BaseRot = 0f;
    float RecoveryAmount = 1f;
    float RecoveryTransparency = 1f;
    int Dir = 0;
    int LastHitNPC = -1;
    public override string Texture => "Everware/Content/PreHardmode/Kiln/Gear/Kilnpost";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.width = 90;
        Projectile.height = 90;
    }
    public override void NetOnHitEnemy(NPC npc)
    {
        base.NetOnHitEnemy(npc);
        if (State == "Thrust")
        {
            LastHitNPC = npc.whoAmI;
        }
    }
    public override void AI()
    {
        TwoHanded = false;
        if (Owner.itemAnimation == Owner.itemAnimationMax / 3 || Owner.ItemAnimationJustStarted)
        {
            Projectile.ai[1] = 0f;
            Projectile.ai[2] = 70f;
            Rotation = Owner.AngleTo(NetworkOwner.MousePosition);
            BaseRot = Rotation;
            Dir = Math.Sign(NetworkOwner.MousePosition.X - Owner.MountedCenter.X);
            State = (State == "Spin") ? "Thrust" : "Spin";
            if (State == "Thrust")
            {
                FrontArmExtension = 1f;
                SoundEngine.PlaySound(Kilnpost.ThrustSound, Owner.MountedCenter);
                HitFrames = 2;
                Projectile.damage = 10;
            }
            else
            {
                Projectile.damage = 3;
                BackArmExtension = 0.8f;
            }
        }

        if (Owner.ItemAnimationEndingOrEnded && HasMouseBeenReleased && State != "Breakaway")
        {
            BreakOffSpear();
        }

        if (State == "Spin")
            SpinMotion();
        if (State == "Thrust")
            ThrustMotion();
        if (State == "Breakaway")
            BreakawayMotion();

        base.AI();
    }
    public bool CollidingRightNow()
    {
        if (LastHitNPC != -1 && Main.npc[LastHitNPC].active)
        {
            if (Projectile.Colliding(Projectile.Hitbox, Main.npc[LastHitNPC].Hitbox)) return true;
        }

        return false;
    }
    public void SpawnBreakawaySpearIn(int npcWhoAmI)
    {
        SoundEngine.PlaySound(Kilnpost.BreakawaySound, Projectile.Center);
        RecoveryAmount = 0f;
        RecoveryTransparency = 0f;
        State = "Breakaway";

        for (int i = 0; i < 10; i++)
        {
            Dust.NewDustPerfect(Projectile.Center + new Vector2(10, 0).RotatedBy(Projectile.rotation), ModContent.DustType<RawKilnPowderDust>(), new Vector2(Main.rand.NextFloat(2), 0).RotatedByRandom(MathHelper.TwoPi));
        }

        NPC npc = Main.npc[npcWhoAmI];

        npc.velocity += new Vector2(12, -12).RotatedBy(Projectile.rotation) * npc.knockBackResist;

        ScreenEffects.AddScreenShake(npc.Center, 3f, 0.5f);

        npc.SimpleStrikeNPC(8, Owner.direction, false);

        Projectile proj = Projectile.NewProjectileDirect(new EntitySource_Misc("Breakaway Spear"), npc.Center, Vector2.Zero, ModContent.ProjectileType<KilnpostBreakaway>(), 0, 0f, Projectile.owner, npcWhoAmI);
        proj.rotation = Projectile.rotation;
        (proj.ModProjectile as KilnpostBreakaway).Pos = (Projectile.Center - npc.Center) / 1.2f;
    }
    public void BreakOffSpear()
    {
        if (CollidingRightNow())
        {
            Persist = true;
            if (Projectile.ai[0] < -10)
            {
                SpawnBreakawaySpearIn(LastHitNPC);
            }
        }
        else
        {
            if (LastHitNPC != -1 && Main.npc[LastHitNPC].active)
            {
                if (Projectile.Colliding(new Rectangle(Projectile.Hitbox.X - 20, Projectile.Hitbox.Y - 20, Projectile.Hitbox.Width + 40, Projectile.Hitbox.Height + 40), Main.npc[LastHitNPC].Hitbox))
                {
                    SpawnBreakawaySpearIn(LastHitNPC);
                    return;
                }
            }
        }
    }
    public void BreakawayMotion()
    {
        AutoDirection = false;

        Projectile.damage = 0;

        Owner.direction = Dir;
        Rotation = Rotation.AngleLerp(BaseRot - MathHelper.ToRadians(120f * Dir), 0.5f);
        RotationOffset = RotationOffset.AngleLerp(MathHelper.ToRadians(45f + (130f * Dir)), 0.5f);

        Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 25f, 0.4f);
        Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], 10f, 0.4f);

        if (Projectile.ai[2] < 26f)
        {
            RecoveryAmount = MathHelper.Lerp(RecoveryAmount, 1f, 0.3f);
            if (RecoveryAmount > 0.85f)
            {
                RecoveryTransparency = MathHelper.Lerp(RecoveryTransparency, 1f, 0.2f);
                if (RecoveryTransparency > 0.9f)
                    Projectile.Kill();
            }
        }

        FrontArmExtension = MathHelper.Lerp(FrontArmExtension, 0f, 0.4f);

        Origin = new Vector2(0f, Asset.Frame().Height) + new Vector2(Projectile.ai[1], -Projectile.ai[1]);
        Offset = new Vector2(Projectile.ai[2] / 2, 0).RotatedBy(Rotation);
    }
    public void ThrustMotion()
    {
        Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 20f, 0.6f);

        if (Projectile.ai[2] < 32)
        {
            Projectile.damage = 8;
        }

        RotationOffset = MathHelper.ToRadians(45f);

        FrontArmExtension = MathHelper.Lerp(FrontArmExtension, 0.6f, 0.2f);
        BackArmExtension = MathHelper.Lerp(BackArmExtension, 0f, 0.4f);

        Origin = Asset.Frame().Size() / 2f;
        Offset = new Vector2(Projectile.ai[2], 0).RotatedBy(Rotation);
    }
    public void SpinMotion()
    {
        Projectile.damage = 2;

        Projectile.ai[1]++;

        if (Projectile.ai[1] % 4 == 0 && Projectile.ai[1] < 20)
        {
            HitFrames = 2;
            SoundEngine.PlaySound(SoundID.Item1.WithPitchOffset(0.8f - (Projectile.ai[1] * 0.05f)).WithVolumeScale(1f - (Projectile.ai[1] * 0.038f)), Owner.Center);
        }

        FrontArmExtension = Easing.KeyFloat(Projectile.ai[1], 0f, 20f, 1f, 0.2f, Easing.OutBack, 0.2f);
        BackArmExtension = MathHelper.Lerp(BackArmExtension, 0f, 0.05f);

        float spinSpeed = Easing.KeyFloat((float)Owner.itemAnimation, 0f, (float)Owner.itemAnimationMax, 0f, 0.7f, Easing.Linear);

        FrontArmRotationOffset = (float)Math.Sin(GlobalTimer.Value) * MathHelper.ToRadians(spinSpeed * 20);
        if (Projectile.ai[1] > 20)
        {
            FrontArmRotationOffset = 0f;
        }

        Origin = Asset.Frame().Size() / 2f;
        Offset = new Vector2(FrontArmExtension * 15, 0).RotatedBy(Rotation);
        RotationOffset = MathHelper.ToRadians(135f) + Spin;
        Spin += Owner.direction * spinSpeed;
    }
    public static readonly Asset<Texture2D> BrokenAsset = ModContent.Request<Texture2D>("Everware/Content/PreHardmode/Kiln/Gear/Kilnpost_Broken");
    public override bool PreDraw(ref Color lightColor)
    {
        if (RecoveryAmount < 1f)
            Main.EntitySpriteDraw(BrokenAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, Frame, lightColor, Projectile.rotation, Origin, Scale, Effects);
        else
            Main.EntitySpriteDraw(Asset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, Frame, lightColor, Projectile.rotation, Origin, Scale, Effects);

        return false;
    }
}
public class KilnpostBreakaway : EverProjectile
{
    public Vector2 Pos = Vector2.Zero;
    float Shake = 3f;
    float Out = 3f;


    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
    }

    public override void AI()
    {
        Projectile.tileCollide = false;
        Out *= 0.7f;
        Shake *= 0.7f;
        if (Projectile.ai[0] != 0)
            Projectile.Center = Main.npc[(int)Projectile.ai[0]].Center + Pos + new Vector2(Shake, 0).RotatedByRandom(MathHelper.TwoPi) + new Vector2(3 - (Out * 3), 0).RotatedBy(Projectile.rotation + MathHelper.ToRadians(135f));
        else Projectile.Kill();

        if (!Main.npc[(int)Projectile.ai[0]].active)
        {
            Projectile.Kill();
        }

        Projectile.ai[1]++;
        if (Projectile.ai[1] > 100)
        {
            Projectile.alpha += 20;
            if (Projectile.alpha > 255) Projectile.Kill();
        }

        if (Projectile.ai[1] % 15 == 14 && Projectile.ai[1] < 90)
        {
            Shake = 3f;
            Main.npc[(int)Projectile.ai[0]].SimpleStrikeNPC(1, 0);
        }
    }

    public override bool? CanDamage()
    {
        return false;
    }
}