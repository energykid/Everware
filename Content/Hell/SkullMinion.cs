using Everware.Common.Systems;
using Everware.Content.Base.Buffs;
using Everware.Content.Base.Projectiles;
using Everware.Core.Projectiles;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace Everware.Content.Hell;

class SkullMinion : EverMinion
{
    public override string Texture => "Everware/Assets/Textures/Hell/SkullMinion";
    public override int BuffType => ModContent.BuffType<SkullMinionBuff>();

    public override void NetOnSpawn()
    {
        base.NetOnSpawn();
        Projectile.velocity = new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-10, 10));
        Projectile.tileCollide = false;
        Projectile.Opacity = 0f;
        Projectile.damage = 0;
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.ai[1] = Main.rand.NextFloat(100f);
        Projectile.ai[0] = 29;
        Projectile.netUpdate = true;
    }
    public override void NetOnHitEnemy(NPC npc)
    {
        Projectile.hide = true;
        Projectile.Opacity = 0f;
        Projectile.ai[0] = 0;
        ScreenEffects.AddScreenShake(npc.Center, 2f);
        SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.ObsidianSkullExplode.Asset.WithPitchOffset(Main.rand.NextFloat(0.3f, 0.5f)), npc.Center);
        Projectile.NewProjectileDirect(new EntitySource_Parent(Projectile, "Skull Minion Explosion"), npc.Center, Vector2.Zero,
            ModContent.ProjectileType<SkullMinionExplosion>(), Projectile.damage * 2 / 3, 0f, Projectile.owner);
        Projectile.Center = Owner.Center + new Vector2(Main.rand.NextFloat(-200, 200), Main.rand.NextFloat(-200, 200));
        Projectile.netUpdate = true;
    }
    NPC targetNPC;
    public override void AI()
    {
        Projectile.damage = 0;

        base.AI();

        Projectile.timeLeft = 10;
        Projectile.ai[1]++;
        if (Projectile.ai[0] > 30)
        {
            Projectile.hide = false;
        }
        if (Projectile.ai[0] > 60)
        {
            if (BehaviorUtils.ClosestNPC(ref targetNPC, 650, Owner.Center, hostilesOnly: true, ignoreTiles: true))
            {
                if (Projectile.ai[0] > 90)
                {
                    if (Projectile.ai[0] == 91)
                    {
                        SoundStyle style = Assets.Sounds.Gear.Weapon.ObsidianSkullDash.Asset with
                        {
                            MaxInstances = 20,
                            PitchVariance = 0.2f
                        };
                        SoundEngine.PlaySound(style, Projectile.Center);
                    }
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(targetNPC.Center) * (Projectile.ai[0] - 60) * 2f, 0.1f);
                    Projectile.damage = 50;
                }
            }
            else
            {
                Projectile.ai[0]--;
                if (Projectile.ai[0] >= 60)
                {
                    Projectile.ai[0] = 59;
                }

                float a = (float)(Math.Sin(Projectile.ai[0] / 10f));

                Projectile.velocity = Vector2.Lerp(Projectile.Center, Owner.Center + new Vector2((float)(Math.Sin(Projectile.ai[1] / 22f)) * 155f, (float)(Math.Sin(Projectile.ai[1] / 23f)) * 125f), 0.05f) - Projectile.Center;

                Projectile.position.Y += a;
            }
        }
        else
        {
            float a = (float)(Math.Sin(Projectile.ai[0] / 10f));

            Projectile.velocity = Vector2.Lerp(Projectile.Center, Owner.Center + new Vector2((float)(Math.Sin(Projectile.ai[1] / 22f)) * 155f, (float)(Math.Sin(Projectile.ai[1] / 23f)) * 125f), 0.05f) - Projectile.Center;

            Projectile.position.Y += a;

            if (Projectile.ai[0] == 30)
            {
                SoundStyle style = Assets.Sounds.Gear.Weapon.ObsidianSkullSummon.Asset with
                {
                    MaxInstances = 20,
                    PitchVariance = 0.5f
                };
                SoundEngine.PlaySound(style, Projectile.Center);
            }

            Projectile.damage = 0;
        }

        Projectile.ai[0]++;

        if (!Projectile.hide)
            Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.1f);

        Projectile.rotation = Projectile.velocity.ToRotation();
        //Projectile.velocity.X += 0.5f;
    }
    public static readonly Asset<Texture2D> TrailTexture = Assets.Textures.Misc.PerlinNoise.Asset;
    public static readonly Asset<Texture2D> MainTexture = Assets.Textures.Hell.SkullMinion.Asset;
    public static readonly Asset<Texture2D> GlowmaskTexture = Assets.Textures.Hell.SkullMinion_Glow.Asset;
    public override void PostDraw(Color lightColor)
    { }
    public override bool PreDraw(ref Color lightColor)
    {
        List<Vector2> vec2s = [];
        List<Color> cols = [];
        List<Vector2> texcoords = [];

        for (int i = 0; i < TrailLength - 1; i++)
        {
            float rot = Projectile.oldPos[i].AngleTo(Projectile.oldPos[i + 1]);

            float a = (float)(TrailLength - 1);

            vec2s.Add(Projectile.oldPos[i] + (Projectile.Size / 2f) + new Vector2(0, -10).RotatedBy(rot));
            vec2s.Add(Projectile.oldPos[i] + (Projectile.Size / 2f) + new Vector2(0, 10).RotatedBy(rot));
            texcoords.Add(new Vector2(0, (float)i / a));
            texcoords.Add(new Vector2(1, (float)i / a));
            cols.Add(Color.White);
            cols.Add(Color.White);
        }

        var Effect = Assets.Effects.Hell.SkullFireEffect.CreateSkullFire();

        Effect.Parameters.NoiseTexture = TrailTexture.Value;
        Effect.Parameters.Progress = (-Projectile.ai[0] / 10) % 1f;
        Effect.Parameters.ExplosionColor1 = new Vector4(1f, 1f, 0f, 1f);
        Effect.Parameters.ExplosionColor2 = new Vector4(0.7f, 0.3f, 0f, 0.2f);
        Effect.Parameters.ExplosionColorMid = new Vector4(0f, 0f, 0f, 0.2f);

        Effect.Apply();

        float s = Projectile.velocity.Length() / TrailTexture.Height();
        PixelRendering.DrawPixelatedSprite(TrailTexture.Value, Projectile.Center - Main.screenPosition, TrailTexture.Frame(3), Color.White, Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f), new Vector2(TrailTexture.Width() / 6, 0), new(0.25f, s * 5f), effect: Effect.Shader);

        SpriteEffects eff = Projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

        Projectile.frameCounter++;
        if (Projectile.frameCounter > 5)
        {
            Projectile.frame++;
            if (Projectile.frame >= 7) Projectile.frame = 0;
            Projectile.frameCounter = 0;
        }

        Rectangle frame = MainTexture.Frame(1, 7, 0, Projectile.frame);
        Vector2 orig = frame.Size() / 2f;
        if (Projectile.scale > 0.7f)
        {
            DrawingUtils.DrawGlowBehind(Projectile, Color.Orange.MultiplyRGBA(new(Projectile.Opacity, Projectile.Opacity, Projectile.Opacity, 0f)), new Vector2(-2, -5).RotatedBy(Projectile.rotation), eff, 2 + MathHelper.Lerp(50, 0, Projectile.Opacity), frame);
        }
        SpriteEffectRendering.DrawSprite(MainTexture.Value, Projectile.Center - Main.screenPosition, frame, Color.Lerp(lightColor, Color.White, 0.2f).MultiplyRGBA(new(Projectile.Opacity, Projectile.Opacity, Projectile.Opacity, Projectile.Opacity)), Projectile.rotation, orig, new(Projectile.scale), eff);
        SpriteEffectRendering.DrawSprite(GlowmaskTexture.Value, Projectile.Center - Main.screenPosition, frame, Color.White.MultiplyRGBA(new(Projectile.Opacity, Projectile.Opacity, Projectile.Opacity, Projectile.Opacity)), Projectile.rotation, orig, new(Projectile.scale), eff);

        return false;
    }
}
class SkullMinionExplosion : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Hell/SkullMinionExplosion";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.projFrames[Projectile.type] = 7;
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.penetrate = -1;
        Projectile.width = Projectile.height = 68;
        Projectile.rotation = Main.rand.NextFloat(-0.2f, 0.2f);
        Projectile.friendly = true;
        Projectile.frame = 0;
    }
    public override void NetOnHitEnemy(NPC npc)
    {
        base.NetOnHitEnemy(npc);
        Projectile.damage = 0;
        npc.AddBuff(BuffID.OnFire, 50);
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White.MultiplyRGBA(new(Projectile.Opacity, Projectile.Opacity, Projectile.Opacity, Projectile.Opacity));
    }
    public override void AI()
    {
        Projectile.ai[0] += 0.4f;
        if (Projectile.ai[0] >= 3) Projectile.Opacity *= 0.8f;

        if (Projectile.ai[0] >= 7) Projectile.Kill();

        Projectile.frame = (int)Math.Floor(Projectile.ai[0]);
    }
}
class SkullMinionBuff : EverMinionBuff
{
    public override string Texture => "Everware/Assets/Textures/Hell/SkullMinionBuff";
    public override int MinionID => ModContent.ProjectileType<SkullMinion>();
}