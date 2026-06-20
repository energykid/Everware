using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Core.Projectiles;
using Everware.Utils;
using System.Collections.Generic;
using Terraria.ID;

namespace Everware.Content.Reliquary.ChiseledStatues;

public class GreatGargoyle : EverWeaponItem
{
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/GreatGargoyle";

    public override int Rarity => ItemRarityID.Blue;

    public override int? HoldoutType => ModContent.ProjectileType<GreatGargoyleHoldout>();

    public static int SummonDamage => 60;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToBasicWeapon(40, 90, DamageClass.Melee);
    }
    public override void SetStaticDefaults()
    {
        ChiselablesList.AllChiselables.Add(new(ItemID.ImpStatue, Type));
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        return base.PreDrawTooltipLine(line, ref yOffset);
    }
    static int DamageTooltipLineIndex = 0;
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        base.ModifyTooltips(tooltips);

        for (int i = 0; i < tooltips.Count; i++)
        {
            if (tooltips[i].Name == "Damage")
            {
                DamageTooltipLineIndex = i + 1;

                TooltipLine line = new TooltipLine("Extra Summon Damage", "(" + SummonDamage.ToString() + " " + Mods.Everware.Items.GreatGargoyle.SummonDamage.GetText() + ")")
                {
                    OverrideColor = new Color(0.8f, 1f, 1f)
                };

                tooltips.Insert(i + 1, line);
                break;
            }
        }
    }
}

public class GreatGargoyleHoldout : EverHoldoutProjectile
{
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/GreatGargoyle";

    public override void SetDefaults()
    {
        base.SetDefaults();
        TwoHanded = true;
        Projectile.width = 70;
        Projectile.height = 64;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.netUpdate = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        Vector2 cen = Owner.MountedCenter + new Vector2(50, 0).RotatedBy(Owner.AngleTo(NetworkOwner.MousePosition));

        hitbox = new Rectangle((int)cen.X - 60, (int)cen.Y - 60, 120, 120);
        base.ModifyDamageHitbox(ref hitbox);
    }

    float Glow = 0f;

    int Pause = 0;
    int Dmg = 0;

    public override bool SinglePersistent => true;
    public override bool PreAI()
    {
        return true;
    }
    bool Flip = false;
    public override void AI()
    {
        Pause--;
        if (Pause == 0)
        {
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact.WithPitchOffset(0.2f), Projectile.Center);
        }

        Projectile.timeLeft = 10;

        Flip = Timer >= 50 && Timer < 80;

        float speed = Owner.GetAttackSpeed(DamageClass.Melee);

        if (Pause <= 0)
        {
            Timer += speed;

            if (Timer.ValueAt(42, speed) || Timer.ValueAt(72, speed))
            {

                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
                HitFrames = 2;
            }
        }
        else
        {
            Timer += 0.01f;
            Owner.itemAnimation++;
            Owner.itemTime++;
        }

        float rotOff = Easing.KeyFloat(Timer, 0, 20, -100, -100, Easing.InOutQuart);
        rotOff = Easing.KeyFloat(Timer, 20, 40, -100, -60, Easing.InQuad, rotOff);
        rotOff = Easing.KeyFloat(Timer, 40, 60, -60, 180, Easing.OutBack, rotOff);
        rotOff = Easing.KeyFloat(Timer, 60, 70, 180, 160, Easing.InQuad, rotOff);
        rotOff = Easing.KeyFloat(Timer, 70, 91, 160, -100, Easing.OutBack, rotOff);

        float sc = 1f + (Math.Abs(RotationOffset - MathHelper.ToRadians(rotOff)) * 0.3f);
        if (Timer <= 10) sc = 1f;

        if (Pause <= 0)
        {
            Rotation = Owner.AngleTo(NetworkOwner.MousePosition);
            Rotation += (float)Math.Sin(GlobalTimer.Value / 10f) * 0.05f;
            Glow = (sc - 1f) * 2f;
            Scale = new Vector2(sc, sc);
        }

        RotationOffset = MathHelper.ToRadians(rotOff);
        FrontArmRotationOffset = RotationOffset - MathHelper.ToRadians(45f);
        BackArmRotationOffset = (RotationOffset / 2f) - MathHelper.ToRadians(45f);

        Offset = new Vector2(5, 0).RotatedBy(Rotation + RotationOffset);

        base.AI();

        if (Flip)
        {
            Projectile.rotation += MathHelper.ToRadians(90f);
            Origin = new Vector2(70, 64);
            Effects = SpriteEffects.FlipHorizontally;
        }
        else
        {
            Origin = new Vector2(8, 64);
            Effects = SpriteEffects.None;
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Pause = 10;
        SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact.WithPitchOffset(-0.7f), Projectile.Center);
        base.OnHitNPC(target, hit, damageDone);
    }
    public override void NetOnHitEnemy(NPC npc)
    {
        base.NetOnHitEnemy(npc);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var asset = Assets.Textures.Reliquary.ChiseledStatues.GreatGargoyle.Asset;
        var glowAsset = Assets.Textures.Reliquary.ChiseledStatues.GreatGargoyle_Glow.Asset;
        var slashAsset = Assets.Textures.Misc.Slash.Asset;

        Main.EntitySpriteDraw(asset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, asset.Frame(), lightColor, Projectile.rotation, Origin, Scale, Effects);
        Main.EntitySpriteDraw(glowAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, glowAsset.Frame(), Color.White.MultiplyRGBA(new Color(Glow * 2f, Glow * 4f, Glow * 2f, 0f)), Projectile.rotation, Origin, Scale, Effects);

        if (Glow > 0.1f)
        {
            Main.EntitySpriteDraw(slashAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, slashAsset.Frame(), Color.SeaGreen.MultiplyRGBA(new Color(Glow * 0.2f, Glow * 0.2f, Glow * 0.2f, 0f)), Projectile.rotation - MathHelper.ToRadians((Flip ? 135 : 45)), new Vector2(-40f, 84f), Scale * 0.7f, SpriteEffects.None);

            Main.EntitySpriteDraw(slashAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, slashAsset.Frame(), Color.Black.MultiplyRGBA(new Color(Glow * 0.2f, Glow * 0.2f, Glow * 0.2f, Glow * 0.2f)), Projectile.rotation - MathHelper.ToRadians((Flip ? 135 : 45)), new Vector2(-40f, 84f), Scale * 1.05f, SpriteEffects.None);

            Main.EntitySpriteDraw(slashAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, slashAsset.Frame(), Color.Lime.MultiplyRGBA(new Color(Glow * 0.4f, Glow * 0.4f, Glow * 0.4f, 0f)), Projectile.rotation - MathHelper.ToRadians((Flip ? 135 : 45)), new Vector2(-40f, 84f), Scale, SpriteEffects.None);
        }

        return false;
    }
}