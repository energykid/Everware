using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Content.Misc.Particles;
using Everware.Core.Projectiles;
using Everware.Utils;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;

namespace Everware.Content.Reliquary.ChiseledStatues;

public class GreatGargoyle : EverWeaponItem
{
    public static Color GreenYellow => new Color(110, 214, 0);

    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/GreatGargoyle";

    public override int Rarity => ItemRarityID.Blue;

    public override int? HoldoutType => ModContent.ProjectileType<GreatGargoyleHoldout>();

    public static int SummonDamage => 22;

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

    public override void SendExtraAI(BinaryWriter writer)
    {
        base.SendExtraAI(writer);
        writer.Write(Owner.GetModPlayer<GreatGargoylePlayer>().GargoyleCharges);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        base.ReceiveExtraAI(reader);
        Owner.GetModPlayer<GreatGargoylePlayer>().GargoyleCharges = reader.ReadInt32();
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
    public bool ShouldRunScare => Owner.GetModPlayer<GreatGargoylePlayer>().GargoyleCharges >= 4;
    public bool Scare = false;
    public override void AI()
    {
        if (!Started) Scare = ShouldRunScare;

        float speed = Owner.GetAttackSpeed(DamageClass.Melee);
        if (!Scare)
        {
            Pause--;
            if (Pause == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact.WithPitchOffset(0.2f), Projectile.Center);
            }

            Projectile.timeLeft = 10;

            Flip = Timer >= 50 && Timer < 80;

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

            if (Owner.GetModPlayer<GreatGargoylePlayer>().GargoyleCharges < 0)
                Scale *= Easing.KeyFloat(Timer, 0, 20, 0f, 1f, Easing.OutExpo, 1f);
            if (ShouldRunScare || !NetworkOwner.MouseDown)
                Scale *= Easing.KeyFloat(Timer, 60, 91, 1f, 0f, Easing.InExpo, 1f);
        }
        else
        {
            Timer += speed;

            Vector2 off = Easing.KeyVector2(Timer, 0, 40, Vector2.Zero, new Vector2(0, -70), Easing.OutBack, new Vector2(0, -70));
            off = Easing.KeyVector2(Timer, 40, 91, new Vector2(0, -70), Vector2.Zero, Easing.InExpo, off);

            Offset = off + new Vector2((float)Math.Sin(GlobalTimer.Value / 20f), (float)Math.Sin(GlobalTimer.Value / 21f));

            Glow = Easing.KeyFloat(Timer, 0, 10, 0.5f, 0f, Easing.InCirc, 0f);

            FrontArmExtension = Easing.KeyFloat(Timer, 0, 40, 0f, 1f, Easing.OutExpo, 1f);

            Rotation = new Vector2(0, -1).ToRotation() + (float)Math.Sin(GlobalTimer.Value / 10f) * 0.01f;

            ExtraRotationOffset = Easing.KeyFloat(Timer, 0, 40, MathHelper.ToRadians(360 * Owner.direction), MathHelper.ToRadians(45), Easing.OutBack, MathHelper.ToRadians(45));
            ExtraRotationOffset = Easing.KeyFloat(Timer, 40, 91, MathHelper.ToRadians(45), MathHelper.ToRadians(45 + (-60 * Owner.direction)), Easing.InExpo, ExtraRotationOffset);

            Origin = new Vector2(70 / 2, 64 / 2);

            Owner.GetModPlayer<GreatGargoylePlayer>().GargoyleCharges = 0;

            Scale *= Easing.KeyFloat(Timer, 70, 91, 1f, 0f, Easing.InExpo, 1f);

            if (Timer.ValueAt(3, speed))
            {
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
                SoundEngine.PlaySound(SoundID.NPCHit52, Projectile.Center);
            }
            if (Timer.ValueAt(40, speed))
            {
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
                SoundEngine.PlaySound(SoundID.NPCHit54.WithPitchOffset(-0.7f), Projectile.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath17, Projectile.Center);
            }

            if ((Timer % 6).ValueAt(2, speed) && Timer > 40 && Timer < 60)
            {
                float Radius = 200;

                if (!Main.dedServ)
                {
                    foreach (var npc in Main.ActiveNPCs)
                    {
                        if (npc.Distance(Owner.Center) < Radius && npc.IsHostile())
                        {
                            if (Timer.ValueAt(56, speed))
                            {
                                if (!npc.GetGlobalNPC<GreatGargoyleTagNPC>().Tagged)
                                    npc.GetGlobalNPC<GreatGargoyleTagNPC>().Tag(npc, 10);
                            }
                            npc.StrikeNPC(GreatGargoyle.SummonDamage + Main.rand.Next(-10, 10), 0f, 0);
                        }
                    }
                }
            }
            if (Timer > 40 && Timer < 50)
            {

                for (int i = 0; i < 3; i++)
                    new SmallSmoke(Projectile.Center, new Vector2(Main.rand.NextFloat(8f), 0).RotatedByRandom(MathHelper.TwoPi), Color.Black) { Scale = new Vector2(1.4f) }.Spawn();

                ScreenEffects.AddScreenShake(Owner.Center, 1f, 0.7f);
                ScreenEffects.DimScreen(0.2f);
                ScreenEffects.ZoomScreen(0.1f);
            }

            base.AI();
        }
    }
    float ExtraRotationOffset = 0f;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        ScreenEffects.AddScreenShake(target.Center, 7f, 0.7f);
        Pause = 5;
        Owner.GetModPlayer<GreatGargoylePlayer>().GargoyleCharges++;
        SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact.WithPitchOffset(-0.7f), Projectile.Center);
        base.OnHitNPC(target, hit, damageDone);
    }
    public override void NetOnHitEnemy(NPC npc)
    {
        Projectile.netUpdate = true;
        base.NetOnHitEnemy(npc);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var pixel = Assets.Textures.Misc.SinglePixel.Asset;

        var asset = Assets.Textures.Reliquary.ChiseledStatues.GreatGargoyle.Asset;
        var glowAsset = Assets.Textures.Reliquary.ChiseledStatues.GreatGargoyle_Glow.Asset;
        var slashAsset = Assets.Textures.Misc.Slash.Asset;

        float glow = 0f;
        glow = Easing.KeyFloat(Timer, 0, 30, 0f, 0.2f, Easing.OutElastic, glow);
        glow = Easing.KeyFloat(Timer, 30, 40, 0.2f, 0.8f, Easing.InExpo, glow);
        glow = Easing.KeyFloat(Timer, 40, 70, 0.8f, 0f, Easing.InOutExpo, glow);

        if (Scare)
        {
            DrawingUtils.DrawGlowWithPadding(asset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, asset.Frame(), GreatGargoyle.GreenYellow.MultiplyRGBA(new Color(glow / 3f, glow / 3f, glow / 3f, 0f)), Projectile.rotation + ExtraRotationOffset, Origin, Scale, Effects, 0.01f + (glow * 0.1f));
        }
        Main.EntitySpriteDraw(asset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, asset.Frame(), lightColor, Projectile.rotation + ExtraRotationOffset, Origin, Scale, Effects);
        Main.EntitySpriteDraw(glowAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, glowAsset.Frame(), Color.White.MultiplyRGBA(new Color(Glow * 2f, Glow * 4f, Glow * 2f, 0f)), Projectile.rotation + ExtraRotationOffset, Origin, Scale, Effects);

        if (Glow > 0.1f)
        {
            Main.EntitySpriteDraw(slashAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, slashAsset.Frame(), Color.SeaGreen.MultiplyRGBA(new Color(Glow * 0.2f, Glow * 0.2f, Glow * 0.2f, 0f)), Projectile.rotation + ExtraRotationOffset - MathHelper.ToRadians((Flip ? 135 : 45)), new Vector2(-20f, 84f), Scale * 0.7f, SpriteEffects.None);

            Main.EntitySpriteDraw(slashAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, slashAsset.Frame(), Color.Black.MultiplyRGBA(new Color(Glow * 0.2f, Glow * 0.2f, Glow * 0.2f, Glow * 0.2f)), Projectile.rotation + ExtraRotationOffset - MathHelper.ToRadians((Flip ? 135 : 45)), new Vector2(-20f, 84f), Scale * 1.05f, SpriteEffects.None);

            Main.EntitySpriteDraw(slashAsset.Value, Owner.MountedCenter + Offset + new Vector2(0, Owner.gfxOffY) - Main.screenPosition, slashAsset.Frame(), GreatGargoyle.GreenYellow.MultiplyRGBA(new Color(Glow, Glow, Glow, Glow * 0.6f)), Projectile.rotation + ExtraRotationOffset - MathHelper.ToRadians((Flip ? 135 : 45)), new Vector2(-20f, 84f), Scale, SpriteEffects.None);
        }

        return false;
    }
}

public class GreatGargoylePlayer : ModPlayer
{
    public int GargoyleCharges = 0;
}