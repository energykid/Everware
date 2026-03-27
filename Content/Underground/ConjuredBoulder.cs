using Everware.Content.Base.ParticleSystem;
using Everware.Content.Misc.Particles;
using Everware.Core.Projectiles;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;

namespace Everware.Content.Underground;

public class ConjuredBoulder : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Underground/ConjuredBoulder";
    public override void SetDefaults()
    {
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 2000;
        Projectile.width = Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.hide = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
    }
    float length = 16;
    public override void NetOnSpawn()
    {
        SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.BookOfBouldersConjure.Asset.WithPitchOffset(Main.rand.NextFloat(-0.05f, 0.05f)), Projectile.Center);

        for (int i = 0; i < 6; i++)
        {
            Easing.AnimationCurve curve = Main.rand.NextBool() ? Easing.OutBack : Easing.OutExpo;

            GenericParticle part = new GenericParticle(
                Projectile.Center,
                Vector2.Zero,
                Vector2.One,
                Particle =>
                {
                    Particle.Center = Projectile.Center + new Vector2(Easing.KeyFloat(Particle.ai[1], 0f, length, 0f, Particle.ai[2], curve, 0f), 0).RotatedBy(Particle.Rotation);
                    Particle.Rotation += Particle.ai[0];
                    Particle.ai[1]--;
                    if (Particle.ai[1] < 0) Particle.Kill();
                },
                Particle =>
                {
                    Asset<Texture2D> FragmentTexture = Assets.Textures.Underground.ConjuredBoulderFragment.Asset;
                    Main.EntitySpriteDraw(FragmentTexture.Value, Particle.Center - Main.screenPosition, FragmentTexture.Frame(),
                        Lighting.GetColor(((Particle.Center) / 16).ToPoint()), Particle.Rotation * 1.5f, FragmentTexture.Size() / 2, Particle.Scale * Easing.KeyFloat(Particle.ai[1], 0, length, Particle.Scale.X, 0f, Easing.InSine, Particle.Scale.X), SpriteEffects.None);
                })
            {
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi)
            };
            part.ai[0] = Main.rand.NextFloat(-0.06f, 0.06f);
            part.ai[1] = length;
            part.ai[2] = Main.rand.NextFloat(20, 45);
            part.Spawn();
        }
    }
    public override void AI()
    {
        base.AI();
        Projectile.ai[0]++;
        if (Projectile.ai[0] == length)
        {
            SoundEngine.PlaySound(SoundID.Dig.WithPitchOffset(Main.rand.NextFloat(-0.5f, -0.2f)), Projectile.Center);
            Projectile.tileCollide = true;
        }
        if (Projectile.ai[0] > length)
        {
            Projectile.hide = false;
            Projectile.rotation += MathHelper.ToRadians(Projectile.velocity.Y);
            Projectile.velocity.Y += (Projectile.ai[0] - length - 3) * 0.2f;
        }
        else if (Projectile.ai[0] < length - 5)
        {
            Vector2 vel = new Vector2(Main.rand.NextFloat(3, 6), 0).RotatedByRandom(MathHelper.TwoPi);
            TextureSparkParticle part = new TextureSparkParticle(Projectile.Center - (vel * 12f), vel, vel.ToRotation() + MathHelper.PiOver2, "Everware/Assets/Textures/Misc/Spark", new Vector2(0.2f, 1f), new(18, 18), (new(0.75f, 0.75f, 0.75f, 0.5f)), false,
                a => { a.Scale *= 0.8f; })
            {
                AffectedByLight = true,
                Pixelated = true
            };
            part.Scale = part.Scale * (Projectile.ai[0] / length) * 2f;
            part.Spawn();
        }
    }
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox = new Rectangle((int)Projectile.Center.X - 20, (int)Projectile.Center.Y - 20, 40, 40);
    }
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        width = 10;
        height = 10;
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.Center = Projectile.Center.Grounded();
        for (int i = 0; i < 7; i++)
        {
            new SmallSmoke(Projectile.Center, new Vector2(Main.rand.NextFloat(5), 0).RotatedByRandom(MathHelper.TwoPi), new Color(0.1f, 0.1f, 0.1f, 0.35f)).Spawn();
        }
        for (int i = 0; i < 6; i++)
        {
            GenericParticle part = new GenericParticle(
                Projectile.Center,
                Vector2.Zero,
                Vector2.One,
                Particle =>
                {
                    Particle.velocity.Y += 0.45f; Particle.velocity.X *= 0.96f;
                    Particle.Center += Particle.velocity;
                    Particle.Rotation += Particle.ai[0];
                    Particle.ai[1]++;
                    if (Particle.ai[1] > length) Particle.Kill();
                },
                Particle =>
                {
                    Asset<Texture2D> FragmentTexture = Assets.Textures.Underground.ConjuredBoulderFragment.Asset;
                    Main.EntitySpriteDraw(FragmentTexture.Value, Particle.Center - Main.screenPosition, FragmentTexture.Frame(),
                        Lighting.GetColor(((Particle.Center) / 16).ToPoint()), Particle.Rotation * 1.5f, FragmentTexture.Size() / 2, Particle.Scale * Easing.KeyFloat(Particle.ai[1], 0, length, Particle.Scale.X, 0f, Easing.InSine, Particle.Scale.X), SpriteEffects.None);
                })
            {
                velocity = new Vector2(Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-6, -3))
            };
            part.ai[0] = Main.rand.NextFloat(-0.06f, 0.06f);
            part.ai[1] = 0;
            part.ai[2] = Main.rand.NextFloat(20, 45);
            part.Spawn();
        }

        SoundEngine.PlaySound(SoundID.Item50.WithPitchOffset(Main.rand.NextFloat(-1f, -0.6f)), Projectile.Center);
        SoundEngine.PlaySound(SoundID.Item50.WithPitchOffset(Main.rand.NextFloat(-1f, -0.6f)), Projectile.Center);
        SoundEngine.PlaySound(SoundID.Dig.WithPitchOffset(Main.rand.NextFloat(-1f, -0.6f)), Projectile.Center);

        return true;
    }
}
