using Everware.Common.Systems;
using Everware.Content.Base.Items;
using Everware.Content.Misc.Particles;
using Everware.Core.Projectiles;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace Everware.Content.Hell;

public class HotShot : EverWeaponItem
{
    public override string Texture => "Everware/Assets/Textures/Hell/HotShot";
    public override int? HoldoutType => ModContent.ProjectileType<HotShotHoldout>();
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToBasicWeapon(24, 60, DamageClass.Ranged);
        Item.autoReuse = true;
        Item.useAmmo = AmmoID.Bullet;
        Item.width = Assets.Textures.Hell.HotShot.Asset.Width();
        Item.height = Assets.Textures.Hell.HotShot.Asset.Height();
    }
    public int ChargeLevel = 0;
    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
    public override bool? UseItem(Player player)
    {
        if (player.altFunctionUse == 2 && player.ItemAnimationJustStarted)
        {
            ChargeLevel += 1;
            ChargeLevel = Math.Clamp(ChargeLevel, 0, 4);
        }
        return base.UseItem(player);
    }
    public override bool CanUseItem(Player player)
    {
        Item.useTime = 45;
        Item.useAnimation = 45;
        if (player.altFunctionUse == 2)
        {
            Item.useTime = 18;
            Item.useAnimation = 18;
        }
        return base.CanUseItem(player);
    }
}

public class HotShotHoldout : EverHoldoutProjectile
{
    bool AltF = false;
    public override string Texture => "Everware/Assets/Textures/Hell/HotShot";
    public override LocalizedText DisplayName => Language.GetText("Mods.Everware.Items.HotShot.DisplayName");

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
    public float GetChargeAmount()
    {
        if (Owner.HeldItem.ModItem is HotShot shot)
        {
            return (float)(shot.ChargeLevel);
        }
        return 0f;
    }

    float RotLerpFactor = 1f;
    float PumpFrame = 0f;
    float ChargeFrame = 0f;
    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;

        AmmoType = AmmoID.Bullet;

        TwoHanded = true;

        FrontArmExtension = 1f;

        RotLerpFactor = MathHelper.Lerp(RotLerpFactor, 0.5f, 0.05f);

        Projectile.ai[1]++;

        ChargeFrame = 0;
        if (Owner.HeldItem.ModItem is HotShot shot)
        {
            ChargeFrame = shot.ChargeLevel;
        }

        if (Owner.altFunctionUse == 2 || AltF)
        {
            AltF = true;
            if (GetChargeAmount() > 0)
            {
                Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.025f);
            }

            if (Projectile.ai[1] == 1)
            {
                Rotation = Owner.AngleTo(NetworkOwner.MousePosition).AngleLerp(Vector2.Zero.AngleTo(new Vector2(0, 2)), 0.5f);
                RotationOffset = MathHelper.ToRadians(30 * Owner.direction);
                Pump();
            }

            float ItemAnim = Owner.itemAnimationMax;

            PumpFrame = Projectile.ai[1] / ItemAnim * 6;

            Rotation = Rotation.AngleLerp(Owner.AngleTo(NetworkOwner.MousePosition).AngleLerp(Vector2.Zero.AngleTo(new Vector2(0, 2)), 0.5f), 0.2f);

            RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(20 * Owner.direction), 0.5f);

            Offset = new Vector2(-10, 0).RotatedBy(Rotation);

            FrontArmExtension = Easing.KeyFloat(Projectile.ai[1], 0f, ItemAnim, 0.7f, 1f, Easing.InCirc);

            FrontArmRotationOffset = RotationOffset;
            BackArmRotationOffset = RotationOffset;
        }
        else
        {
            if (Projectile.ai[1] == 1)
            {
                Fire();
            }
            float ItemAnim = Owner.itemAnimationMax;

            if (Projectile.ai[1] == 3)
            {
                if (Owner.HeldItem.ModItem is HotShot sh)
                {
                    sh.ChargeLevel = 0;
                }
            }

            if (GetChargeAmount() > 0)
            {
                Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.025f);
            }

            float rotOffset = 0f;
            rotOffset = Easing.KeyFloat(Projectile.ai[1], 0f, ItemAnim / 8f, 0f, 30f, Easing.OutCirc, rotOffset);
            rotOffset = Easing.KeyFloat(Projectile.ai[1], ItemAnim / 8f, ItemAnim / 8f * 2, 30f, 20f, Easing.Linear, rotOffset);
            rotOffset = Easing.KeyFloat(Projectile.ai[1], ItemAnim / 8f * 2, ItemAnim / 5f * 2f, 20f, 0f, Easing.OutCirc, rotOffset);

            Rotation = Rotation.AngleLerp(Owner.AngleTo(NetworkOwner.MousePosition), RotLerpFactor);

            RotationOffset = MathHelper.ToRadians(rotOffset) * (float)-Owner.direction;

            Offset = Easing.KeyVector2(Projectile.ai[1], 0f, ItemAnim / 3f, Vector2.Zero, new Vector2(-5, 0).RotatedBy(Rotation), Easing.OutCirc, Offset);
            Offset = Easing.KeyVector2(Projectile.ai[1], ItemAnim / 3f, ItemAnim / 3f * 2f, new Vector2(-5, 0).RotatedBy(Rotation), new Vector2(-2, 0).RotatedBy(Rotation) + new Vector2(0, 3), Easing.OutCirc, Offset);
            Offset = Easing.KeyVector2(Projectile.ai[1], ItemAnim / 3f * 2f, ItemAnim, new Vector2(-2, 0).RotatedBy(Rotation) + new Vector2(0, 3), Vector2.Zero, Easing.InCirc, Offset);

            FrontArmExtension = Easing.KeyFloat(Projectile.ai[1], 0f, ItemAnim, 0.7f, 1f, Easing.InCirc);

            FrontArmRotationOffset = RotationOffset;
            BackArmRotationOffset = RotationOffset;
        }
        base.AI();
    }

    public void Pump()
    {
        SoundStyle st = Assets.Sounds.Gear.Weapon.HotShotPump3.Asset;
        if (GetChargeAmount() == 1) st = Assets.Sounds.Gear.Weapon.HotShotPump1.Asset;
        if (GetChargeAmount() == 2) st = Assets.Sounds.Gear.Weapon.HotShotPump2.Asset;
        if (GetChargeAmount() == 4) st = Assets.Sounds.Gear.Weapon.HotShotMax.Asset;

        SoundEngine.PlaySound(st.WithVolumeScale(0.75f), Owner.Center);
    }

    public void Fire()
    {
        SoundEngine.PlaySound(SoundID.Item70.WithPitchOffset(0.5f), Projectile.Center);

        SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.HotShotFire.Asset.WithPitchVariance(0.2f), Owner.Center);

        Item? it = UseAmmo(Owner.HeldItem, Owner);

        if (it != null)
        {
            {
                float charge = GetChargeAmount() / 3f;
                charge = Math.Clamp(charge, 0f, 1f);
                Vector2 blastlocation = new Vector2(40, 0).RotatedBy(Rotation);
                float spread = MathHelper.Lerp(20, 4, charge);
                float amt = MathHelper.Lerp(3, 6, charge);
                float speed = MathHelper.Lerp(12, 25, charge);
                for (int i = 0; i < amt; i++)
                {
                    Vector2 v = new Vector2(20, 0).RotatedBy(Rotation);
                    if (Collision.CanHitLine(Owner.Center, 2, 2, Owner.Center + v, 2, 2)) v = Vector2.Zero;
                    Projectile.NewProjectile(new EntitySource_Parent(Projectile, "Hot Shot fire"), Owner.Center + v, new Vector2(speed, 0).RotatedBy(Owner.AngleTo(NetworkOwner.MousePosition)).RotatedByRandom(MathHelper.ToRadians(spread)), it.shoot, Projectile.damage, 4, Projectile.owner);
                }
                if (GetChargeAmount() <= 2)
                {
                    ScreenEffects.AddScreenShake(Owner.Center, 5f, 0.5f);
                    Projectile.NewProjectile(new EntitySource_Parent(Projectile, "Hot Shot blast"), Owner.Center + blastlocation, Vector2.Zero, ModContent.ProjectileType<HotShotBurst>(), Projectile.damage * (int)amt, 4, Projectile.owner);
                }
                else
                {
                    ScreenEffects.AddScreenShake(Owner.Center, 10f, 0.5f);
                    Vector2 vel = Owner.DirectionFrom(NetworkOwner.MousePosition) * 10;
                    vel *= new Vector2(0.5f, 0.75f);
                    Owner.velocity += vel;
                    NetMessage.SendData(MessageID.PlayerControls, number: Owner.whoAmI);
                    Projectile.NewProjectile(new EntitySource_Parent(Projectile, "Hot Shot blast"), Owner.Center + blastlocation, Vector2.Zero, ModContent.ProjectileType<HotShotBurstLarge>(), Projectile.damage * (int)amt, 4, Projectile.owner);
                }

                Lighting.AddLight(Projectile.Center, 0.6f, 0.4f, 0.1f);

                for (int i = 0; i < 10; i++)
                {
                    new SmallSmoke(Owner.Center + (new Vector2(40, 0).RotatedBy(Owner.AngleTo(NetworkOwner.MousePosition))), new Vector2(Main.rand.Next(10), 0).RotatedBy(Owner.AngleTo(NetworkOwner.MousePosition)).RotatedByRandom(MathHelper.ToRadians(20f)), new Color(0f, 0f, 0f, 0.2f)).Spawn();
                }
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Effects = Owner.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
        Origin = Owner.direction == -1 ? new Vector2(7, 22 - 16) : new Vector2(7, 16);

        Asset<Texture2D> ChargeTex = Assets.Textures.Hell.HotShot_Charge.Asset;
        Asset<Texture2D> ChargeGlowTex = Assets.Textures.Hell.HotShot_Charge_Glow.Asset;
        Asset<Texture2D> PumpTex = Assets.Textures.Hell.HotShot_Pump.Asset;

        Rectangle ChargeFr = ChargeTex.Frame(1, 5, 0, (int)Math.Floor(MathHelper.Clamp(ChargeFrame, 0f, 3f)));
        Rectangle PumpFr = PumpTex.Frame(1, 7, 0, (int)Math.Floor(PumpFrame));

        Main.EntitySpriteDraw(ChargeTex.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, ChargeFr, lightColor, Projectile.rotation, Origin, Scale, Effects);
        Main.EntitySpriteDraw(ChargeGlowTex.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, ChargeFr, Color.White, Projectile.rotation, Origin, Scale, Effects);
        Main.EntitySpriteDraw(PumpTex.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, PumpFr, lightColor, Projectile.rotation, Origin, Scale, Effects);

        return false;
    }
}

public class HotShotBurst : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Hell/HotShotBurst";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.damage = 0;
        Projectile.width = 66; Projectile.height = (112 / 4);
    }
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.projFrames[Type] = 4;
    }
    public override void AI()
    {
        Projectile.damage = 0;
        Projectile.velocity = Owner.velocity;
        Projectile.frameCounter += 1;
        if (Projectile.frameCounter > 3)
        {
            Projectile.frame += 1;
            if (Projectile.frame > 3) Projectile.Kill();
        }
        Projectile.Center = Owner.Center + new Vector2(-10, -10 * Owner.direction).RotatedBy(Owner.AngleTo(NetworkOwner.MousePosition)) + (Owner.DirectionTo(NetworkOwner.MousePosition) * 25);
        Projectile.rotation = Owner.AngleTo(NetworkOwner.MousePosition);
    }
}

public class HotShotBurstLarge : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Hell/HotShotBurstLarge";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.damage = 0;
        Projectile.width = 94; Projectile.height = (230 / 5);
    }
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.projFrames[Type] = 5;
    }
    public override void AI()
    {
        Projectile.damage = 0;
        Projectile.velocity = Owner.velocity;
        Projectile.frameCounter += 1;
        if (Projectile.frameCounter > 3)
        {
            Projectile.frame += 1;
            if (Projectile.frame > 4) Projectile.Kill();
        }
        Projectile.Center = Owner.Center + new Vector2(-10, -10 * Owner.direction).RotatedBy(Owner.AngleTo(NetworkOwner.MousePosition)) + (Owner.DirectionTo(NetworkOwner.MousePosition) * 35);
        Projectile.rotation = Owner.AngleTo(NetworkOwner.MousePosition);
    }
}