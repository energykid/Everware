using Everware.Core.Projectiles;
using Terraria.ID;

namespace Everware.Content.Base.Projectiles;

public class EverCustomDrawProjectile : EverProjectile
{
    public override string Texture => "Everware/icon_small";
    public delegate void CustomDraw(Player player, float direction);
    public delegate void CustomSwing(Player player, Projectile projectile);
    public delegate void CustomEnemyHit(Player player, NPC target);
    public CustomDraw customDraw;
    public CustomSwing customSwing;
    public CustomEnemyHit customHit;
    public int itemType = -1;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.damage = 0;
        Projectile.tileCollide = false;
        Projectile.netUpdate = true;
    }
    public override void NetOnSpawn()
    {
        Projectile.netUpdate = true;
    }
    public override void NetOnHitEnemy(NPC target)
    {
        if (customHit != null)
        {
            customHit(Owner, target);
        }
        Projectile.netUpdate = true;
    }
    public int timer = 0;
    public override void AI()
    {
        timer++;

        Player player = Main.player[Projectile.owner];
        Projectile.position = player.Center;
        if ((!Owner.ItemAnimationActive) && (NetworkOwner.AltFunction != 2) && timer > 5)
        {
            Projectile.Kill();
        }
        player.heldProj = Projectile.whoAmI;

        if (Main.netMode != NetmodeID.Server)
        {
            Projectile.ai[1] = Projectile.Center.AngleTo(Main.MouseWorld);
        }

        if (customSwing != null)
        {
            Projectile.damage = Main.player[Projectile.owner].HeldItem.damage;
            Projectile.knockBack = Main.player[Projectile.owner].HeldItem.knockBack;
            customSwing(player, Projectile);
        }

        Projectile.netUpdate = true;
    }
    public override bool? CanDamage()
    {
        return false;
    }
    public override bool? CanCutTiles()
    {
        return false;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}
