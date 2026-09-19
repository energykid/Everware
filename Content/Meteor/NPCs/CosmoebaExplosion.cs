using Everware.Content.Base.Projectiles;
using Everware.Content.Misc.Particles;
using static Everware.Content.Meteor.NPCs.Cosmoeba;

namespace Everware.Content.Meteor.NPCs;

public class CosmoebaExplosion : EverExplosionProjectile
{
    public override int FrameCount => base.FrameCount;
    public override string Texture => base.Texture;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.width = Projectile.height = 100;
        Projectile.damage = 40;
        Projectile.knockBack = 5f;
        Projectile.ai[2] = 0f;
        Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }
    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
        base.ModifyHitPlayer(target, ref modifiers);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var asset = Assets.Textures.Meteor.NPCs.CosmoebaExplosion.Asset;

        var fr = asset.Frame(1, FrameCount, 0, Projectile.frame);

        var effect = Assets.Effects.Meteor.NPCs.CosmoebaExplosion.CreateEffect();
        effect.Parameters.Clip = Projectile.ai[2];
        effect.Parameters.Resolution = fr.Size() * 2f;
        effect.Parameters.Frame = Projectile.frame;
        effect.Parameters.NoiseTexture = Assets.Textures.Misc.PerlinNoise.Asset.Value;
        effect.Apply();

        Main.spriteBatch.End(out var sb);
        Main.spriteBatch.Begin(sb with { CustomEffect = effect.Shader });

        Main.EntitySpriteDraw(asset.Value, Projectile.Center - Main.screenPosition, fr, Color.White, Projectile.rotation, fr.Size() / 2f, 1f, SpriteEffects.None);

        Main.spriteBatch.Restart(sb);

        return false;
    }
    public override void AI()
    {
        Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 1f, 0.08f);
        if (Projectile.frame < 3)
        {
            if (Projectile.frame < 2)
            {
                new FizzParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(-60, 60), 0).RotatedByRandom(MathHelper.TwoPi), new Vector2(0, -Main.rand.NextFloat(0, 2)), -1).Spawn();
            }
            new SmallSmoke(Projectile.Center + new Vector2(Main.rand.NextFloat(-40, 40), 0).RotatedByRandom(MathHelper.TwoPi), new Vector2(Main.rand.Next(10), 0).RotatedByRandom(MathHelper.TwoPi), new Color(0f, 0f, 0f, 1f)) { Scale = new Vector2(Main.rand.NextFloat(1f, 2f)) }.Spawn();
        }
        else Projectile.damage = 0;
        base.AI();
    }
}
