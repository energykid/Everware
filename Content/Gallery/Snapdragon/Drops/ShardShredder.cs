using Everware.Common.Systems;
using Everware.Content.Base.Items;
using Everware.Content.Base.ParticleSystem;
using Everware.Content.Misc.Particles;
using Everware.Core.Projectiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace Everware.Content.Gallery.Snapdragon.Drops;

public class ShardShredder : EverWeaponItem
{
    public override string Texture => "Everware/Assets/Textures/Gallery/Snapdragon/Drops/ShardShredder";
    public override int? HoldoutType => ModContent.ProjectileType<ShardShredderProj>();
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToBasicWeapon(10, 2, DamageClass.Ranged);
        Item.autoReuse = true;
        Item.useAmmo = AmmoID.Bullet;
        Item.width = Assets.Textures.Gallery.Snapdragon.Drops.ShardShredder.Asset.Width();
        Item.height = Assets.Textures.Gallery.Snapdragon.Drops.ShardShredder.Asset.Height();
        Item.value = Sell.Gold(4) + Sell.Silver(75);
        Item.rare = ItemRarityID.Pink;
    }
    public float Fullness = 0;
    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Fullness);
    }
    public override void NetReceive(BinaryReader reader)
    {
        Fullness = reader.ReadSingle();
    }
    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
    public override bool? UseItem(Player player)
    {
        return base.UseItem(player);
    }
    public override bool CanUseItem(Player player)
    {
        Item.useTime = 2;
        Item.useAnimation = 2;
        if (player.altFunctionUse == 2)
        {
            Item.useTime = 55;
            Item.useAnimation = 55;
        }
        return base.CanUseItem(player);
    }
    public override void UseAnimation(Player player)
    {
        player.SendRightClick();
    }
}
public class ShardShredderProj : EverHoldoutProjectile
{
    int Time = 0;
    public override string Texture => "Everware/Assets/Textures/Gallery/Snapdragon/Drops/ShardShredder";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.width = Asset.Width();
        Projectile.height = Asset.Height();
    }
    public override void AI()
    {
        AmmoType = AmmoID.Bullet;
        Time++;
        AutoDirection = true;
        Owner.direction = NetworkOwner.MousePosition.X < Owner.Center.X ? -1 : 1;
        Effects = Owner.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
        Origin = new Vector2(10, Owner.direction == -1 ? 28 : 12);

        TwoHanded = true;
        FrontArmExtension = MathHelper.Lerp(FrontArmExtension, 0.5f, 0.3f);
        BackArmExtension = 1f;
        BackArmRotationOffset = Owner.direction * MathHelper.ToRadians(15f);

        if (Owner.RightClicking())
        {
            if (Owner.HeldItem.ModItem is ShardShredder shsh)
            {
                shsh.Fullness = MathHelper.Lerp(shsh.Fullness, 1f, 0.1f);
            }
            // Reload animation
            if (Projectile.ai[1] == 0)
            {
                SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.ShardShredder_Reload.Asset, Projectile.Center);
            }
                Rotation = Owner.AngleTo(NetworkOwner.MousePosition);
            Rotation = Rotation.AngleLerp(Rotation + RotationOffset, 0.05f);
            Vector2 IceEjectVel = new Vector2(-1f, 0.2f).RotatedBy(Projectile.rotation);
            if (Projectile.ai[1] < 15 && Projectile.ai[1] > 2)
            {
                Vector2 v = (IceEjectVel * Main.rand.NextFloat(40, 50)).RotatedByRandom(0.2f);
                new SmallMistFade(IcePosition() + (v / 2f), v / 30, Color.AliceBlue.MultiplyRGBA(new(1f, 1f, 1f, 0.5f)), new Vector2(0.4f, 0.4f)).Spawn();
            }
            if (Projectile.ai[1] <= 10)
            {
                RotationOffset = MathHelper.Lerp(RotationOffset, Owner.direction * MathHelper.PiOver4 * 0.05f, 0.4f);
            }
            else if (Projectile.ai[1] <= 20)
            {
                RotationOffset = MathHelper.Lerp(RotationOffset, Owner.direction * MathHelper.PiOver4 * 0.15f, 0.2f);
            }
            else
            {
                RotationOffset = MathHelper.Lerp(RotationOffset, Owner.direction * MathHelper.PiOver4 * 0.1f, 0.3f);
                if (Projectile.ai[1] > 45)
                {
                    RotationOffset *= 0.8f;
                    Rotation = Rotation.AngleLerp(Owner.AngleTo(NetworkOwner.MousePosition), 0.25f);
                }
            }
        }
        else
        {
            Rotation = Owner.AngleTo(NetworkOwner.MousePosition);

            Projectile.ai[2] -= 0.5f;
            if (Time == 2)
            {
                Projectile.ai[2] = 4;
                FrontArmExtension = 0;
                Offset = new Vector2(Owner.direction * -5, Main.rand.NextFloat(-1.5f, 1.5f));

                Item? it = UseAmmo(Owner.HeldItem, Owner);

                if (it != null)
                {
                    ScreenEffects.AddScreenShake(Projectile.Center, 2f, 0.1f);
                    Projectile.NewProjectile(new EntitySource_Parent(Projectile, "Shard Shredder bullet"), MuzzlePosition(), new Vector2(20, 0).RotatedBy(Rotation), it.shoot, Projectile.damage, Projectile.knockBack, Projectile.owner);

                    if (GunFullness() > 0.2f)
                    {
                        ScreenEffects.AddScreenShake(Projectile.Center, 4f, 0.4f);
                        Projectile p = Projectile.NewProjectileDirect(new EntitySource_Parent(Projectile, "Shard Shredder bullet"), MuzzlePosition() + (Projectile.rotation.ToRotationVector2() * 45), new Vector2(1f, 0).RotatedBy(Rotation + Main.rand.NextFloat(-0.1f, 0.1f)), ModContent.ProjectileType<ShardShredderIcicle>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        p.friendly = true;
                        p.hostile = false;
                    }
                }

                SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.ShardShredder_Fire.Asset with { Volume = 0.4f, MaxInstances = 18, PitchVariance = 0.2f }, Projectile.Center);

                if (GunFullness() > 0.2f)
                {
                    SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.ShardShredder_Ice.Asset with { Volume = 0.3f, MaxInstances = 18, PitchVariance = 0.2f }, Projectile.Center);
                }
            }
            Offset *= 0.6f;

            if (Owner.HeldItem.ModItem is ShardShredder shsh)
            {
                shsh.Fullness *= 0.99f;
            }
        }

        Projectile.ai[1]++;

        base.AI();
    }
    public Vector2 MuzzlePosition()
    {
        return Owner.MountedCenter + (new Vector2(20, (3 * Owner.direction)).RotatedBy(Rotation));
    }
    public Vector2 IcePosition()
    {
        return Owner.MountedCenter + (new Vector2(10, (12 * Owner.direction)).RotatedBy(Rotation));
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var MainAsset = Assets.Textures.Gallery.Snapdragon.Drops.ShardShredderProj.Asset;
        var IceAsset = Assets.Textures.Gallery.Snapdragon.Drops.ShardShredder_Ice.Asset;
        var FlashAsset = Assets.Textures.Gallery.Snapdragon.Drops.ShardShredder_Flash.Asset;

        var MainFrame = MainAsset.Frame(verticalFrames: 5, frameY: (int)Projectile.ai[2]);
        var IceFrame = IceAsset.Frame(verticalFrames: 6, frameY: 5 - ((int)Math.Floor(GunFullness() * 5)));

        var FlashFrame = FlashAsset.Frame(verticalFrames: 2, frameY: (int)Math.Floor(Time / 3f));

        Main.EntitySpriteDraw(MainAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, MainFrame, lightColor, Projectile.rotation, Origin, Scale, Effects);
        Main.EntitySpriteDraw(IceAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, IceFrame, Color.Lerp(lightColor, Color.White, GunFullness() / 2f), Projectile.rotation, Origin, Scale, Effects);

        Vector2 flashOrigin = Origin + new Vector2(-66, Owner.direction == 1 ? -6 : -18);

        if (!Owner.RightClicking())
            Main.EntitySpriteDraw(FlashAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, FlashFrame, new Color(1f, 1f, 1f, 0.5f), Rotation, flashOrigin, Scale, Effects);
        
        return false;
    }
    public float GunFullness()
    {
        if (Owner.HeldItem.ModItem is ShardShredder shsh)
        {
            return shsh.Fullness;
        }
        return 0f;
    }
}