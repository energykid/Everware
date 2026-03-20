using Everware.Content.Base.ParticleSystem;
using Everware.Core.Projectiles;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;

namespace Everware.Content.MiscWeapons.Mage;

public class ManaSpike : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/MiscWeapons/ManaSpike";
    Vector2 targetPosition = Vector2.Zero;
    Vector2 visualTargetPosition = Vector2.Zero;
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 2000;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.hide = true;
        Projectile.penetrate = -1;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.WritePackedVector2(targetPosition);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        targetPosition = reader.ReadPackedVector2();
    }
    public override void AI()
    {
        Projectile.damage = 0;

        if (Projectile.ai[0] > MathHelper.Lerp(Projectile.ai[1], Projectile.ai[0], 0.2f))
        {
            targetPosition = Projectile.Center.Grounded();
            visualTargetPosition = targetPosition;
        }

        if (Projectile.ai[0] > Projectile.ai[1])
        {
            float c = NetworkOwner.MousePosition.Y;
            c = MathHelper.Clamp(c, Owner.Center.Y - 100, Owner.Center.Y + 100);

            Projectile.Center = new Vector2(NetworkOwner.MousePosition.X, c);
            targetPosition = Projectile.Center.Grounded();
            visualTargetPosition = Vector2.Lerp(visualTargetPosition, targetPosition, 0.1f);
            Projectile.netUpdate = true;
        }
        else
        {
            Projectile.width = 60;
            Projectile.height = 140;
            visualTargetPosition = targetPosition;
            Projectile.Center = visualTargetPosition + new Vector2(0, -70);
            if (Projectile.ai[0] > Projectile.ai[1] - 10)
                Projectile.damage = 24;
        }

        Projectile.ai[0]--;

        if (Projectile.ai[0] == Projectile.ai[1])
        {
            for (int i = 0; i < Main.rand.Next(10, 15); i++)
            {
                new TextureFlashParticle(targetPosition + new Vector2(Main.rand.NextFloat(-30, 30), Main.rand.NextFloat(-140, 0)), Vector2.Zero, new Vector2(0.5f, Main.rand.NextFloat(2f, 6f)), Assets.Textures.Misc.BasicDust.Asset)
                {
                    Pixelated = true,
                    UpdateFunction = c =>
                    {
                        c.velocity.Y -= 0.15f;
                        c.Scale.Y *= 0.95f;
                        if (Math.Abs(c.Scale.Y) < 0.1f) c.Kill();
                    },
                    Color = Color.Purple.MultiplyRGBA(new(1f, 1f, 1f, 0f))
                }.Spawn();
            }

            SoundStyle sound = Assets.Sounds.Gear.Weapon.UnwieldyStaffSpike.Asset;

            sound.MaxInstances = 3;

            SoundEngine.PlaySound(sound, Projectile.Center);

            Projectile.ai[2] = 2;
        }

        Projectile.netUpdate = Projectile.ai[0] % 3 == 0;

        if (Projectile.ai[0] < (-Projectile.ai[1]))
        {
            Projectile.Kill();
        }
    }
    public override bool? CanCutTiles()
    {
        return Projectile.ai[0] < Projectile.ai[1];
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Asset<Texture2D> rayTexture = Assets.Textures.Misc.LightRay.Asset;
        Asset<Texture2D> spikeTexture = Assets.Textures.MiscWeapons.ManaSpike.Asset;

        float outreach = 0.4f;

        float time = MathHelper.Lerp(1f, 0f, Projectile.ai[0] / Owner.itemAnimationMax);

        float colorIntensity = 0f;

        outreach = Easing.KeyFloat(time, 0f, 0.7f, 0.6f, 1f, Easing.OutCirc, outreach);
        outreach = Easing.KeyFloat(time, 0.7f, 1f, 2f, 0.8f, Easing.InOutSine, outreach);

        colorIntensity = Easing.KeyFloat(time, 0f, 0.7f, 0f, 0.2f, Easing.InCirc, colorIntensity);
        colorIntensity = Easing.KeyFloat(time, 0.7f, 1.3f, 0.3f, 0f, Easing.InOutSine, colorIntensity);

        Lighting.AddLight(visualTargetPosition + new Vector2(0, -10), new Vector3(0.4f, 0.2f, 0.6f) * colorIntensity);

        Main.EntitySpriteDraw(rayTexture.Value, visualTargetPosition - Main.screenPosition + new Vector2(0, 20), rayTexture.Frame(), Color.Purple.MultiplyRGBA(new(colorIntensity, colorIntensity, colorIntensity, 0f)), 0f, new Vector2(rayTexture.Width() / 2, rayTexture.Height()), new Vector2(0.15f, 0.2f * outreach), SpriteEffects.None);

        if (Projectile.ai[2] == 2)
        {
            float time2 = MathHelper.Lerp(1f, 0f, Projectile.ai[0] / Projectile.ai[1]);
            Rectangle frame = spikeTexture.Frame(1, 9, 0, (int)Math.Round(time2 * 9f));
            Main.EntitySpriteDraw(spikeTexture.Value, visualTargetPosition - Main.screenPosition, frame, Color.White, 0f, new(frame.Width / 2, frame.Height - 2), new Vector2(1f, 2f), SpriteEffects.None);
        }

        return false;
    }
}
