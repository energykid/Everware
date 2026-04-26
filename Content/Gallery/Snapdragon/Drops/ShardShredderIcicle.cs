using Everware.Content.Misc.Particles;
using Everware.Core.Projectiles;
using Everware.Utils;
using Terraria.ID;

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

        Projectile.rotation = Projectile.velocity.ToRotation();

        if (Projectile.ai[0] == 0)
        {
            Projectile.scale = Main.rand.NextFloat(0.9f, 1.2f);
            Projectile.ai[1] = 1f;
        }
        if (Projectile.ai[0] == 6)
        {
            Projectile.velocity *= 10f;
        }
        if (Projectile.ai[0] >= 6)
        {
            Projectile.velocity *= 1.05f;
        }
        else
        {
            Projectile.velocity = Owner.DirectionTo(NetworkOwner.MousePosition);
            Projectile.Center = Owner.Center + new Vector2(60, 0).RotatedBy(Projectile.rotation);
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

        Main.EntitySpriteDraw(asset.Value, Projectile.Center - Main.screenPosition, asset.Frame(1, 3, frameY: Frame), Color.White, Projectile.rotation, new Vector2(asset.Width() * 0.25f, 6), new Vector2(Projectile.scale, Projectile.ai[1]) * Scale, SpriteEffects.None);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(sb);

        return false;
    }
}