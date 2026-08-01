using Everware.Common.Players;
using Everware.Common.Systems;
using Everware.Content.Base;
using Everware.Content.Base.Items;
using Everware.Content.Base.ParticleSystem;
using Everware.Core.Projectiles;
using Everware.Utils;
using System.IO;
using Terraria.ID;

namespace Everware.Content.Reliquary.ChiseledStatues;

#region Item
public class DichromaticSkull : EverWeaponItem
{
    public class SkullEmoteParticle : Particle
    {
        Asset<Texture2D> Tex;
        int Owner;
        Player GetOwner => Main.player[Owner];
        public SkullEmoteParticle(Vector2 pos, int owner, int dir) : base(pos, Vector2.Zero, Vector2.One, null, null)
        {
            position = pos;

            Owner = owner;

            Pixelated = true;

            if (dir > 0)
            {
                Rotation = Main.rand.NextFloat(-0.5f, 0.5f);

                velocity = new Vector2(Main.rand.NextFloat(10), Main.rand.NextFloat(-20, 0));
                velocity.Normalize();
                velocity *= Main.rand.NextFloat(6, 10f);

                Tex = Assets.Textures.Reliquary.ChiseledStatues.DichromaticSkullHa.Asset;
                FrameCount = new Vector2(1, 3);
                if (Main.getGoodWorld)
                    FrameNum.Y = Main.rand.Next(1, 3);
            }
            else
            {
                velocity = new Vector2(Main.rand.NextFloat(-10, 0), Main.rand.NextFloat(-5, 0));
                velocity.Normalize();
                velocity *= Main.rand.NextFloat(3f, 5f);
                velocity.Y *= 0.2f;

                Tex = Assets.Textures.Reliquary.ChiseledStatues.DichromaticSkullTears.Asset;
                FrameCount = new Vector2(1, 2);
                FrameNum.Y = Main.rand.Next(2);
            }
        }
        public override void Update()
        {
            base.Update();

            ai[0]++;
            Scale = Easing.KeyVector2(ai[0], 0f, 20f, Vector2.One, Vector2.Zero, Easing.InBack);
            if (ai[0] > 20) Kill();

            position += GetOwner.velocity;

            if (Tex == Assets.Textures.Reliquary.ChiseledStatues.DichromaticSkullHa.Asset)
            {
                Rotation *= 0.95f;

                velocity *= 0.8f;
            }
            else
            {
                velocity.X *= 0.9f;
                Rotation += MathHelper.ToRadians(2f);
                velocity.Y += 0.4f;
            }
        }
        public override void Draw()
        {
            Color c = !AffectedByLight ? Color : Color.MultiplyRGBA(Lighting.GetColor((position / 16f).ToPoint()));
            Rectangle frame = Tex.Frame((int)FrameCount.X, (int)FrameCount.Y, (int)FrameNum.X, (int)FrameNum.Y);

            if (Pixelated)
                PixelRendering.DrawPixelatedSprite(Tex.Value, VisualPosition, frame, c.MultiplyRGBA(new(1f, 1f, 1f, Opacity)), Rotation, Origin != -Vector2.One ? Origin : frame.Size() / 2f, Scale, Effects);
            else
                Main.EntitySpriteDraw(Tex.Value, VisualPosition, frame, c.MultiplyRGBA(new(1f, 1f, 1f, Opacity)), Rotation, Origin != -Vector2.One ? Origin : frame.Size() / 2f, Scale, Effects);
        }
    }

    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/DichromaticSkull";
    public override int Rarity => 6;
    public override int HitCount => 2;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ChiselablesList.AllChiselables.Add(new(ItemID.GloomStatue, Type, ItemID.SoulofSight, 10));
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToBasicWeapon(27, 38, DamageClass.Magic);
        Item.autoReuse = true;
    }
    public override void CustomDraw(Player player, float direction)
    {
        int fr = (int)(MathHelper.Lerp(1f, 0f, (float)player.itemAnimation / (float)player.itemAnimationMax) * 8f) % 4;
        float fr2 = MathHelper.Lerp(1f, 0f, (float)player.itemAnimation / (float)player.itemAnimationMax) * MathHelper.TwoPi;

        var asset = Assets.Textures.Reliquary.ChiseledStatues.DichromaticSkullSad.Asset;

        if (player.direction == 1)
        {
            asset = Assets.Textures.Reliquary.ChiseledStatues.DichromaticSkullHappy.Asset;
        }

        float rot = direction;
        if (player.direction == -1)
        {
            rot += MathHelper.Pi;
            rot += MathHelper.ToRadians(50f);
            rot -= (float)Math.Abs(Math.Sin(fr2)) * -player.direction * 0.4f;
        }
        else
        {
            rot += (float)Math.Abs(Math.Sin(fr2)) * -player.direction * 0.2f;
        }

        float sin = (float)Math.Abs(Math.Sin(fr2));

        Color c2 = Color.OrangeRed;
        if (player.direction < 0) c2 = Color.DeepSkyBlue;
        Color color = Lighting.GetColor((player.Center / 16).ToPoint());
        Rectangle frameRect = asset.Frame(1, 4, 0, fr);
        DrawingUtils.DrawGlowWithPadding(asset.Value, AttackVisualOrigin(player) + new Vector2(0f, player.gfxOffY) - Main.screenPosition + new Vector2(
            (float)Math.Sin(GlobalTimer.Value / 25f) * 3f, sin * -player.direction * 2f), frameRect, c2.MultiplyRGBA(new(0.4f, 0.4f, 0.4f, 0.4f)), rot, frameRect.Size() / 2f, Vector2.One, SpriteEffects.None, 0.05f);
        Main.EntitySpriteDraw(asset.Value, AttackVisualOrigin(player) + new Vector2(0f, player.gfxOffY) - Main.screenPosition + new Vector2(
                (float)Math.Sin(GlobalTimer.Value / 25f) * 3f, sin * -player.direction * 2f), frameRect, color, rot, frameRect.Size() / 2f, 1f, SpriteEffects.None);
    }
    public Vector2 AttackOrigin(Player player)
    {
        Vector2 v = player.Center + new Vector2(18 * player.direction, -10 + (float)Math.Sin(GlobalTimer.Value / 22f) * 3f) + new Vector2(10, 0).RotatedBy(player.AngleTo(player.NetworkHandler().MousePosition));
        if (Collision.CanHitLine(player.Center, 1, 1, v, 1, 1)) return v;
        return player.Center;
    }
    public Vector2 AttackVisualOrigin(Player player)
    {
        Vector2 v = player.Center + new Vector2(18 * player.direction, -10 + (float)Math.Sin(GlobalTimer.Value / 22f) * 3f) + new Vector2(10, 0).RotatedBy(player.AngleTo(player.NetworkHandler().MousePosition));
        return v;
    }
    public void AttackLeft(Player player)
    {
        SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.SkullCry.Asset.WithVolumeScale(0.6f), player.Center);
        SoundEngine.PlaySound(SoundID.Item111, player.Center);

        if (Main.LocalPlayer == player)
        {
            for (int i = 0; i < 2; i++)
            {
                int Proj = Projectile.NewProjectile(new EntitySource_ItemUse(player, Item, "Dichromatic Skull Attack Left"), AttackOrigin(player) + new Vector2(0, i),
                    player.DirectionTo(player.NetworkHandler().MousePosition).RotatedBy(-0.1f).RotatedByRandom(0.2f) * Main.rand.NextFloat(10f, 20f), ModContent.ProjectileType<DichromaticSkullWater>(), 20, 0f, player.whoAmI);
                Main.projectile[Proj].scale = Main.rand.NextFloat(0.7f, 1f);
            }
        }
    }
    public void AttackRight(Player player)
    {
        SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.SkullHA.Asset.WithVolumeScale(0.6f), player.Center);
        SoundEngine.PlaySound(SoundID.Item116, player.Center);

        if (Main.LocalPlayer == player)
            Projectile.NewProjectileDirect(new EntitySource_ItemUse(player, Item, "Dichromatic Skull Attack Right"), AttackOrigin(player),
        player.DirectionTo(player.NetworkHandler().MousePosition) * 10, ModContent.ProjectileType<DichromaticSkullFlame>(), 20, 0f, player.whoAmI).rotation = player.AngleTo(player.NetworkHandler().MousePosition);
    }
    public override bool UseCustomDraw => true;
    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.AngleTo(AttackVisualOrigin(player)) + MathHelper.ToRadians(-90f));
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.ThreeQuarters, player.AngleTo(AttackVisualOrigin(player)) + MathHelper.ToRadians(-90f));

        player.direction = (player.GetModPlayer<NetworkPlayer>().MousePosition.X > player.Center.X) ? 1 : -1;

        if (player.itemAnimation % (player.itemAnimationMax / 2) == ((player.itemAnimationMax / 2) - 1))
        {
            if (player.direction == 1)
            {
                AttackRight(player);
                new SkullEmoteParticle(AttackVisualOrigin(player), player.whoAmI, player.direction).Spawn();
            }
            else
            {
                AttackLeft(player);
                new SkullEmoteParticle(AttackVisualOrigin(player), player.whoAmI, player.direction).Spawn();
                new SkullEmoteParticle(AttackVisualOrigin(player), player.whoAmI, player.direction).Spawn();
                new SkullEmoteParticle(AttackVisualOrigin(player), player.whoAmI, player.direction).Spawn();
            }
        }

        base.UseStyle(player, heldItemFrame);
    }
}
#endregion

#region Debuff Handler
public class DichromaticDebuffHandler : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public bool Fire = false;
    public bool Water = false;
    public override void ResetEffects(NPC npc)
    {
        Fire = false; Water = false;
    }
    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        damage = 3;
        if (Fire) npc.lifeRegen -= 24;
        if (Fire && Water)
        { npc.lifeRegen -= 24; damage += 2; }
    }
    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (Water)
            modifiers.ScalingArmorPenetration += 0.05f;
        if (Water && Fire)
            modifiers.ScalingArmorPenetration += 0.05f;
    }
}
#endregion

#region Flame Attack
public class DichromaticFireDebuff : ModBuff
{
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/DichromaticFireDebuff";
    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.GetGlobalNPC<DichromaticDebuffHandler>().Fire = true;

        DichromaticSkullFlame.FlameLashParticle part = new DichromaticSkullFlame.FlameLashParticle(new Vector2(
            Main.rand.NextFloat(npc.Left.X, npc.Right.X),
            Main.rand.NextFloat(npc.Top.Y, npc.Bottom.Y)), new Vector2(0, -3), new Vector2(0.2f, 0.4f));

        if (Main.rand.NextBool(3)) part.Color = Color.Black;

        part.Spawn(DichromaticSkullFlame.FireLayer);

        base.Update(npc, ref buffIndex);
    }
}
public class DichromaticSkullFlame : EverProjectile
{
    public override void Load()
    {
        On_Main.Update += UpdateFireLayer;
        On_Main.DrawProjectiles += DrawFlames;
    }

    public override void Unload()
    {
        On_Main.Update -= UpdateFireLayer;
        On_Main.DrawProjectiles -= DrawFlames;
    }

    private void DrawFlames(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig(self);
        var flameTarget = ScreenspaceTargetPool.Shared.Rent(
            Main.instance.GraphicsDevice,
            (Width, Height) => (Width / 2, Height / 2)
        );

        using (flameTarget.Scope(clearColor: Color.Transparent))
        {
            Main.spriteBatch.Begin();

            for (int i = 0; i < FireLayer.AllParticles.Count; i++)
            {
                if (FireLayer.AllParticles[i].Color != Color.Black)
                    FireLayer.AllParticles[i].Draw();
            }
            for (int i = 0; i < FireLayer.AllParticles.Count; i++)
            {
                if (FireLayer.AllParticles[i].Color == Color.Black)
                    FireLayer.AllParticles[i].Draw();
            }

            Main.spriteBatch.End();
        }

        var effect = Assets.Effects.Misc.ColorGradientEffect.CreateEffect();
        effect.Parameters.Color = [
            Color.Transparent.ToVector4(),
            Color.OrangeRed.ToVector4() * new Vector4(0.2f, 0.2f, 0.6f, 1f),
            Color.OrangeRed.ToVector4() * 0.8f,
            Color.OrangeRed.ToVector4(),
            Color.Orange.ToVector4(),
            Color.Orange.ToVector4() * 1.5f,
            Color.White.ToVector4(),
            Color.White.ToVector4(),
            ];
        effect.Parameters.ColorNumber = 7;
        effect.Apply();

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, Main._multiplyBlendState, Main.DefaultSamplerState, null, Main.Rasterizer, effect.Shader, Main.GameViewMatrix.ZoomMatrix);

        DrawingUtils.DrawGlowWithPadding(flameTarget.Target, Vector2.Zero, flameTarget.Target.Bounds, new Color(0.2f, 0.0f, 0.0f, 0f), 0f, Vector2.Zero, new Vector2(2f, 2f), SpriteEffects.None, radius: 0.01f);

        Main.EntitySpriteDraw(flameTarget.Target, Vector2.Zero, flameTarget.Target.Bounds, new Color(1f, 0.2f, 0.2f, 0f), 0f, Vector2.Zero, new Vector2(2f, 2f), SpriteEffects.None);

        Main.spriteBatch.End();

        flameTarget.Dispose();
    }
    private void UpdateFireLayer(On_Main.orig_Update orig, Main self, GameTime gameTime)
    {
        orig(self, gameTime);
        if (FireLayer != null)
            FireLayer.Update();
    }

    public static readonly ParticleLayer FireLayer = new(0.5f);
    public class FlameLashParticle : Particle
    {
        float Sc = 0f;
        public FlameLashParticle(Vector2 pos, Vector2 vel, Vector2 scale) : base(pos, vel, scale, null, null)
        {
            ai[1] = Main.rand.NextFloat(0.04f, 0.06f);
            Origin = new Vector2(51, 45);
            FrameCount = new Vector2(1, 11);
            Color = Main.rand.NextBool(3) ? Color.Black : Color.Orange;
            Rotation = vel.ToRotation();
            if (Color == Color.Black)
            {
                FrameNum.Y += 3;
                Scale *= 0.6f;
                position += vel * 4;
            }
        }

        public override void Update()
        {
            base.Update();
            Lighting.AddLight(Center, Color.OrangeRed.ToVector3() * 0.7f);
            ai[0]++;
            if (Color == Color.Black || ai[0] > 2)
            {
                FrameNum.Y = MathHelper.Lerp(FrameNum.Y, 12, ai[1]);
                if (FrameNum.Y >= 11) Kill();
            }
            if (Color != Color.Black && FrameNum.Y > 2f)
            {
                Color = Color.Lerp(Color, Color.Black, 0.03f);
            }
            if (Color == Color.Black)
            {
                Scale *= 0.8f;
                Sc = MathHelper.Lerp(Sc, 1f, 0.2f);
            }
            Sc = MathHelper.Lerp(Sc, 1f, 0.2f);
            velocity *= 0.9f;
        }

        public override Asset<Texture2D> Texture => Assets.Textures.Misc.FlameLashParticle.Asset;

        public override void Draw()
        {
            Rectangle frame = Texture.Frame((int)FrameCount.X, (int)FrameCount.Y, (int)FrameNum.X, Color == Color.Black ? 0 : (int)FrameNum.Y);

            Main.EntitySpriteDraw(Texture.Value, VisualPosition, frame, Color, Rotation, Origin, Scale * new Vector2(1f, Sc) * 0.5f, Effects, 0);
        }
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.hide = true;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 40;
        Projectile.width = Projectile.height = 80;
        Projectile.netUpdate = true;
    }
    public bool CanHit(Vector2 vec)
    {
        return vec.Distance(Projectile.Center) < 400;
    }
    public override void NetOnHitEnemy(NPC npc)
    {
        npc.AddBuff(ModContent.BuffType<DichromaticFireDebuff>(), 240);
    }
    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(ModContent.BuffType<DichromaticFireDebuff>(), 240);
    }
    public override bool? CanHitNPC(NPC target)
    {
        return base.CanHitNPC(target);
    }
    public override bool CanHitPvp(Player target)
    {
        return base.CanHitPvp(target);
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Projectile.rotation);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Projectile.rotation = reader.ReadSingle();
    }
    public override void AI()
    {
        if (Projectile.timeLeft < 20) Projectile.velocity *= 0.8f;
        base.AI();
        if (Projectile.ai[0] <= 10 && Projectile.ai[0] != 0)
        {
            new FlameLashParticle(Projectile.Center,
                new Vector2(Main.rand.NextFloat(2f, 10f), Main.rand.NextFloat(-1f, 1f)).RotatedBy(Projectile.rotation) * Main.rand.NextFloat(1.6f, 2f), new Vector2(Main.rand.NextFloat(0.4f, 1f), Main.rand.NextFloat(0.4f, 1f))).Spawn(FireLayer);
        }
        Projectile.ai[0]++;
        if (Projectile.ai[0] > 56) Projectile.Kill();
    }
}
#endregion

#region Water Attack
public class DichromaticWaterDebuff : ModBuff
{
    public override string Texture => "Everware/Assets/Textures/Reliquary/ChiseledStatues/DichromaticFireDebuff";
    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.GetGlobalNPC<DichromaticDebuffHandler>().Water = true;

        if (Main.rand.NextBool(10))
            new DichromaticSkullWater.BubbleWaterDrip(new Vector2(
                Main.rand.NextFloat(npc.Left.X, npc.Right.X),
                Main.rand.NextFloat(npc.Top.Y, npc.Bottom.Y) - 20)).Spawn();

        base.Update(npc, ref buffIndex);
    }
}
public class DichromaticSkullWater : EverProjectile
{
    public class BubbleWaterDrip : Particle
    {
        public override Asset<Texture2D> Texture => Assets.Textures.Reliquary.ChiseledStatues.DichromaticBubbleDrip.Asset;
        public BubbleWaterDrip(Vector2 pos) : base(pos, Vector2.Zero, Vector2.One, null, null) { }
        public override void Update()
        {
            Scale.X = 1f;
            Scale.Y = velocity.Y / 2;
            velocity.Y += 0.1f;
            if (velocity.Y > 2) Opacity -= 0.05f;
            if (Opacity < 0) Kill();
            base.Update();
        }
    }

    public bool Popped = false;
    public Vector2 Scale = Vector2.One;
    public void DrawBubble()
    { }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.tileCollide = true;
        Projectile.friendly = true;
        Projectile.width = Projectile.height = 60;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 50;
        Projectile.penetrate = -1;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 pos = Projectile.Center - Main.screenPosition;

        var asset = Assets.Textures.Reliquary.ChiseledStatues.DichromaticSkullTearBubble.Asset;

        Rectangle fr = asset.Frame(1, 6, 0, (int)Projectile.ai[2]);

        float sq = (float)Math.Sin(Projectile.ai[0] / 5f) * 0.02f;

        Color c = Color.Lerp(Color.White, Lighting.GetColor((Projectile.Center / 16).ToPoint()), 0.5f);

        DrawingUtils.DrawGlowWithPadding(asset.Value, pos, fr, c.MultiplyRGBA(new(0f, 0f, 0.4f, 0.4f)), 0f, fr.Size() / 2, new Vector2(1f + sq, 1f - sq) * Projectile.scale * Projectile.ai[1], SpriteEffects.None);
        Main.EntitySpriteDraw(asset.Value, pos, fr, c, 0f, fr.Size() / 2, new Vector2(1f + sq, 1f - sq) * Projectile.scale * Projectile.ai[1] * Scale, SpriteEffects.None);

        return false;
    }
    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 0.3f);

        Scale = Vector2.Lerp(Scale, Vector2.One, 0.2f);

        if (Main.rand.NextBool(20)) new BubbleWaterDrip(Projectile.Center + new Vector2(Main.rand.NextFloat(-30, 30), Main.rand.NextFloat(-30, 30))).Spawn();

        Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], 1f, 0.2f);

        Projectile.velocity *= 0.95f;
        Projectile.velocity.Y -= 0.01f;
        Projectile.velocity += new Vector2((float)Math.Sin(Projectile.ai[0] / 15f) * 0.01f, (float)Math.Sin(Projectile.ai[0] / 12f) * 0.01f);
        Projectile.ai[0] += Main.rand.NextFloat(2f);

        if (Projectile.ai[0] > 120)
        {
            if (!Popped)
            {
                SoundEngine.PlaySound(SoundID.Item54.WithPitchOffset(-0.7f).WithPitchOffset(0.3f), Projectile.Center);
            }
            Popped = true;
        }

        if (!Popped)
        {
            Projectile.ai[2] = Projectile.ai[0] % 20 < 10 ? 1 : 0;
        }
        else
        {
            Scale = Vector2.Lerp(Scale, new Vector2(1.5f), 0.2f);
            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 7, 0.1f);
            if (Projectile.ai[2] >= 6) Projectile.Kill();
        }

        foreach (Projectile proj in Main.ActiveProjectiles)
        {
            if (proj.Distance(Projectile.Center) < 60 * Projectile.scale && proj != Projectile)
            {
                Projectile.velocity += proj.DirectionTo(Projectile.Center) * 0.1f;
                proj.velocity -= proj.DirectionTo(Projectile.Center) * 0.1f;
            }
        }

        base.AI();
    }
    public override void NetOnHitEnemy(NPC npc)
    {
        npc.AddBuff(ModContent.BuffType<DichromaticWaterDebuff>(), 240);
    }
    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(ModContent.BuffType<DichromaticWaterDebuff>(), 240);
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (oldVelocity.X != Projectile.velocity.X)
        {
            Projectile.velocity.X = -oldVelocity.X;
            Scale.Y = 1.25f;
            Scale.X = 0.75f;
        }
        if (oldVelocity.Y != Projectile.velocity.Y)
        {
            Projectile.velocity.Y = -oldVelocity.Y;
            Scale.Y = 0.75f;
            Scale.X = 1.25f;
        }

        SoundEngine.PlaySound(SoundID.Item111.WithVolumeScale(0.5f).WithPitchVariance(0.2f).WithPitchOffset(0.8f), Projectile.Center);

        return false;
    }
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        width = height = 20;
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }
}
#endregion