using Everware.Core;
using Everware.Core.Projectiles;
using Microsoft.Xna.Framework.Graphics;

namespace Everware.Content.PreHardmode.EyeOfCthulhuRework;

public class CthulhuBloodProjectile : EverProjectile
{
    float Clip = 1f;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.width = Projectile.height = 20;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.knockBack = 3f;
    }
    public override void AI()
    {
        Projectile.ai[0]++;
        if (Projectile.ai[0] < 20)
        {
            Projectile.velocity *= MathHelper.Lerp(Main.expertMode ? 0.85f : 0.75f, 1f, Projectile.ai[0] / 20f);
        }
        else
        {
            Projectile.velocity *= 1.04f;
        }
        if (Projectile.ai[1] != 1)
        {
            Clip = MathHelper.Lerp(Clip, 0.3f, 0.2f);
            Projectile.rotation = Vector2.Zero.AngleFrom(Projectile.velocity) + MathHelper.PiOver2;
        }
        else
        {
            Projectile.velocity *= 0.95f;
            Clip = MathHelper.Lerp(Clip, 1.1f, 0.05f);
            if (Clip > 1f) Projectile.Kill();
        }
        base.AI();
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.ai[1] = 1;
        Projectile.netUpdate = true;
        Projectile.tileCollide = false;
        Projectile.velocity = oldVelocity * 0.2f;
        return false;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var asset = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.CthulhuBloodProjectile.Asset;

        var BloodEffect = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.CthulhuBloodProjectileEffect.CreateBloodEffect();
        BloodEffect.Parameters.BloodGradient = AssetReferences.Content.PreHardmode.EyeOfCthulhuRework.BloodProjectilePalette.Asset.Value;
        BloodEffect.Parameters.LightingColor = Lighting.GetColor((Projectile.Center / 16f).ToPoint()).ToVector4();
        BloodEffect.Parameters.Timer = Projectile.ai[0] / -10f;
        BloodEffect.Parameters.NoiseTexture = Textures.PerlinNoise.Asset.Value;
        BloodEffect.Parameters.Threshold = Clip;
        BloodEffect.Apply();

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, BloodEffect.Shader, Main.GameViewMatrix.ZoomMatrix);

        Main.spriteBatch.Draw(asset.Value, Projectile.Center - Main.screenPosition, asset.Frame(), Color.White, Projectile.rotation, new Vector2(10, 102), new Vector2(1f, 1f + Projectile.velocity.Length() / 20f), SpriteEffects.None, 0f);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);

        return false;
    }
}
