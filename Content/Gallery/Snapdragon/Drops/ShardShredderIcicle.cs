using Everware.Content.Misc.Particles;
using Everware.Core.Projectiles;

namespace Everware.Content.Gallery.Snapdragon;

public class ShardShredderIcicle : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Gallery/Snapdragon/Drops/ShardShredderIcicle";

    int Frame = 1;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Scale = new Vector2(0.5f, 0f);
        Projectile.width = Projectile.height = 20;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.knockBack = 3f;
        Projectile.tileCollide = true;
        if (!Main.dedServ) Frame = Main.rand.Next(3);
    }
    Vector2 Scale = Vector2.Zero;
    public override void AI()
    {
        float time1 = 6;

        Scale = Vector2.Lerp(Scale, Vector2.One, 0.2f);

        if (Projectile.ai[0] == 0)
        {
            Projectile.scale = Main.rand.NextFloat(0.9f, 1.2f);
        }
        if (Projectile.ai[0] == 6)
        {
            Projectile.velocity *= 3f;
        }
        if (Projectile.ai[0] >= 6)
        {
            if (Main.rand.NextBool(8))
            {
                new AnimationParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-10, 10)), Projectile.velocity * 0.5f, Color.White, Assets.Textures.Gallery.Snapdragon.Drops.Sparkle.Asset, 4)
                {
                    AffectedByLight = false,
                    Color = Color.White,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    Pixelated = true,
                    Scale = new Vector2(Main.rand.NextFloat(0.5f, 1f), Main.rand.NextFloat(0.6f, 1f))
                }.Spawn();
            }

            Projectile.velocity *= 1.05f;
            Projectile.ai[2] *= 0.9f;
            Projectile.rotation = Projectile.ai[1] + Projectile.ai[2];
            Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0).RotatedBy(Projectile.rotation);
        }
        else
        {
            Projectile.rotation = Owner.AngleTo(NetworkOwner.MousePosition) + Projectile.ai[2];
            Projectile.ai[1] = Owner.AngleTo(NetworkOwner.MousePosition);

            Projectile.Center = Owner.Center + new Vector2(68, 0).RotatedBy(Owner.AngleTo(NetworkOwner.MousePosition));

            Projectile.velocity = Projectile.rotation.ToRotationVector2() * 7;
        }

        Projectile.ai[0]++;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var asset = Assets.Textures.Gallery.Snapdragon.Drops.ShardShredderIcicle.Asset;

        var FreezeEffect = Assets.Effects.Gallery.Snapdragon.SnapFreezeProjectileEffect.CreateEffect();
        FreezeEffect.Parameters.FreezeGradient = Assets.Textures.Gallery.Snapdragon.SnapFreezeGradient.Asset.Value;
        FreezeEffect.Parameters.LightingColor = Lighting.GetColor((Projectile.Center / 16f).ToPoint()).ToVector4();
        FreezeEffect.Parameters.NoiseWidth = new Vector2(0.35f, 0.01f);
        FreezeEffect.Parameters.NoiseOffset = new Vector2(Projectile.ai[0] / 150, Projectile.ai[2] / 126);
        FreezeEffect.Parameters.NoiseTexture = Textures.PerlinNoise.Asset.Value;
        FreezeEffect.Parameters.Clip = 0.35f;
        FreezeEffect.Parameters.Dimensions = asset.Size() / 2f;
        FreezeEffect.Apply();

        Main.spriteBatch.End(out var sb);
        Main.spriteBatch.Begin(sb with { CustomEffect = FreezeEffect.Shader });

        Main.EntitySpriteDraw(asset.Value, Projectile.Center - Main.screenPosition, asset.Frame(1, 3, frameY: Frame), Color.White, Projectile.rotation, new Vector2(asset.Width() * 0.25f, 6), new Vector2(Projectile.scale, 1f) * Scale, SpriteEffects.None);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(sb);

        return false;
    }
}