using Everware.Core.Projectiles;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Gallery.Snapdragon;

public class SnapFreezeProjectile : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Gallery/Snapdragon/SnapFreezeProjectile";

    int Frame = 1;
    public override void SetDefaults()
    {
        base.SetDefaults();
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
        float time1 = 9;

        Scale = Easing.KeyVector2(Projectile.ai[0], 0, time1, Vector2.Zero, new Vector2(1f, 1f), Easing.Linear, Scale);
        Scale = Easing.KeyVector2(Projectile.ai[0], time1, 18, new Vector2(3f, 0.25f), new Vector2(1f, 1f), Easing.OutCirc, Scale);

        Projectile.rotation = Projectile.velocity.ToRotation();

        if (Projectile.ai[0] == 0)
        {
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact.WithPitchOffset(0.5f).WithPitchVariance(0.3f), Projectile.Center);
            Projectile.scale = Main.rand.NextFloat(0.9f, 1.8f);
            Projectile.ai[1] = Main.rand.NextFloat(0.9f, 1.1f);
        }

        if (Projectile.ai[0] == time1)
        {
            Projectile.velocity *= 30f;
            SoundStyle a = Assets.Sounds.NPC.Snapdragon_SnapFreeze.Asset with { MaxInstances = 5 };
            SoundEngine.PlaySound(a.WithPitchVariance(0.3f).WithPitchOffset(Projectile.ai[2] / 60), Projectile.Center);
        }

        if (Projectile.ai[0] > time1 && Projectile.ai[0] < 18) Projectile.velocity *= 0.7f;
        else
        {
            if (Projectile.ai[0] > 18)
                Projectile.velocity *= 1.07f;
            else Projectile.velocity *= 0.78f;
        }

        Projectile.ai[0]++;

        base.AI();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var asset = Assets.Textures.Gallery.Snapdragon.SnapFreezeProjectile.Asset;

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

        Main.EntitySpriteDraw(asset.Value, Projectile.Center - Main.screenPosition, asset.Frame(1, 3, frameY: Frame), Color.White, Projectile.rotation, new Vector2(asset.Width() * 0.25f, 12), new Vector2(Projectile.scale, Projectile.ai[1]) * Scale, SpriteEffects.None);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(sb);

        return false;
    }
}