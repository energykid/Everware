using Everware.Content.Misc.Particles;
using static Everware.Content.Meteor.NPCs.Cosmoeba;

namespace Everware.Content.Base.Projectiles;

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
        Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }
    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Projectile.Center.X);
        base.ModifyHitPlayer(target, ref modifiers);
    }
    public override void AI()
    {
        if (Projectile.frame < 3)
        {
            if (Projectile.frame < 2)
            {
                new FizzParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(-60, 60), 0).RotatedByRandom(MathHelper.TwoPi), new Vector2(0, -Main.rand.NextFloat(0, 2)), -1).Spawn();
            }
            new SmallSmoke(Projectile.Center + new Vector2(Main.rand.NextFloat(-40, 40), 0).RotatedByRandom(MathHelper.TwoPi), new Vector2(Main.rand.Next(10), 0).RotatedByRandom(MathHelper.TwoPi), new Color(0f, 0f, 0f, 1f)) { Scale = new Vector2(Main.rand.NextFloat(1f, 2f)) }.Spawn();
        }
        base.AI();
    }
}
