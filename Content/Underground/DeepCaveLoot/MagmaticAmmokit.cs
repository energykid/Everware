using Everware.Content.Base.Items;
using Everware.Core.Projectiles;
using Everware.Utils;
using System.Collections.Generic;
using Terraria.ID;

namespace Everware.Content.Underground.DeepCaveLoot;

public class MagmaticAmmokit : EverItem
{
    public override int Rarity => 3;
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/MagmaticAmmokit";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToAccessory(30, 24);
        Item.value = Sell.Gold(1) + Sell.Silver(35);
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);
        player.GetModPlayer<MagmaticAmmokitPlayer>().Enabled = true;
    }
}

public class MagmaticAmmokitPlayer : ModPlayer
{
    public bool Enabled = false;
    public override void ResetEffects()
    {
        Enabled = false;
    }
    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (proj.DamageType == DamageClass.Ranged && hit.Crit)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath14.WithPitchOffset(1f), proj.Center);

            Projectile proj2 = Projectile.NewProjectileDirect(new EntitySource_Parent(target, "Magmatic Ammokit"), proj.Center, Vector2.Zero, ModContent.ProjectileType<MagmaticExplosion>(),
                hit.Damage * 2 / 3, 2f, proj.owner, 0, target.whoAmI);
            proj2.rotation = proj.velocity.ToRotation();
        }
    }
}

public class MagmaticExplosion : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/FlamePierceIn";
    Vector2 DrawPosition = Vector2.Zero;
    bool Start = false;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.projFrames[Type] = 4;
    }
    Vector2 vel = Vector2.Zero;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.hide = false;
        Projectile.width = 40;
        vel = new Vector2(Main.rand.NextFloat(2f), 0).RotatedByRandom(MathHelper.TwoPi);
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 70;
    }
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        Vector2 inc = new Vector2(2, 0).RotatedBy(Projectile.rotation);

        Vector2 outPosition = Projectile.Center;
        float radius = Target.Size.Length() * 0.35f;
        while (outPosition.Distance(Target.Center) < radius) outPosition += inc;

        Point cen = outPosition.ToPoint();
        cen += new Vector2(Target.Size.Length(), 0).RotatedBy(Projectile.rotation).ToPoint();
        base.ModifyDamageHitbox(ref hitbox);
        hitbox = new Rectangle(cen.X - 30, cen.Y - 30, 60, 60);
    }
    public NPC Target => Main.npc[(int)Projectile.ai[1]];
    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.1f, 0f) * MathHelper.Lerp(6f, 0f, Projectile.ai[0] / 8f));

        Projectile.Center += Target.velocity + vel;
        base.AI();
        vel *= 0.7f;

        Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 10f, 0.1f);

        if (Projectile.ai[0] > 8) Projectile.Kill();
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCs.Add(index);
        base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var FlamePierceIn = Assets.Textures.Underground.DeepCaveLoot.FlamePierceIn.Asset;
        var FlamePierceOut = Assets.Textures.Underground.DeepCaveLoot.FlamePierceOut.Asset;

        int inFrames = 4;
        int outFrames = 8;

        int frameNum = (int)Projectile.ai[0];
        int frameNum2 = (int)(Projectile.ai[0] / 2f);

        float radius = Target.Size.Length() * 0.35f;

        Vector2 inc = new Vector2(2, 0).RotatedBy(Projectile.rotation);

        Vector2 outPosition = Projectile.Center;
        while (outPosition.Distance(Target.Center) < radius) outPosition += inc;

        Vector2 inPosition = Projectile.Center;
        while (inPosition.Distance(Target.Center) < radius) inPosition -= inc;

        DrawingUtils.EverEntitySpriteDraw(FlamePierceIn, inPosition - Main.screenPosition, new Vector2(12, 10), Projectile.rotation, inFrames, frameNum2);

        DrawingUtils.EverEntitySpriteDraw(FlamePierceOut, outPosition - Main.screenPosition, new Vector2(4, 14), new Vector2(2f, 1f), Projectile.rotation, outFrames, frameNum);

        return false;
    }
}
