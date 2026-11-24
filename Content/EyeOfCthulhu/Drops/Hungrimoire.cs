using System.Collections.Generic;
using Everware.Content.Base.Items;
using Everware.Content.Base.ParticleSystem;
using Everware.Content.EyeOfCthulhuRework;
using Everware.Core.Projectiles;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.EyeOfCthulhu.Drops;

public class Hungrimoire : EverTomeWeapon
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToMagicWeapon(0, 40, 0, true);
        Item.mana = 10;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.damage = 30;
        Item.value = Sell.Gold(1) + Sell.Silver(50);
    }
    public override int FrameCount => 5;
    public override string Texture => "Everware/Assets/Textures/EyeOfCthulhu/Drops/Hungrimoire";
    public override Asset<Texture2D> TomeAsset => Assets.Textures.EyeOfCthulhu.Drops.Hungrimoire_Holdout.Asset;

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        if (player.itemAnimation == player.itemAnimationMax - 2)
        {
            SoundEngine.PlaySound(SoundID.Item170.WithPitchOffset(1f).WithPitchVariance(0.2f) with {MaxInstances = 2}, player.Center);
            player.direction = (player.NetworkHandler().MousePosition.X > player.Center.X) ? 1 : -1;
            
            Projectile.NewProjectileDirect(new EntitySource_Parent(player, "Hungrimoire"),
                player.Center + new Vector2(0, -50), new Vector2(6 * player.direction, 0),
                ModContent.ProjectileType<HungrimoireToothSpawner>(), 0, 0, player.whoAmI);
        }

        base.UseStyle(player, heldItemFrame);
    }
}

public class HungrimoireToothSpawner : EverProjectile
{
    public override string Texture => "Everware/Assets/Textures/EyeOfCthulhu/Drops/HungrimoireTeeth";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.hide = true;
        Projectile.timeLeft = 40;
        Projectile.width = Projectile.height = 20;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {
        Projectile.ai[0]++;
        Projectile.velocity.X *= 1.01f;
        if (Projectile.ai[0] >= 35) Projectile.Kill();
        else
        {
            if (Projectile.ai[0] % 5 == 4)
            {
                if (!Main.dedServ)
                {
                    
                    Projectile proj = Projectile.NewProjectileDirect(new EntitySource_Parent(Owner, "Hungrimoire"),
                        Projectile.Center.Grounded(), Vector2.Zero, ModContent.ProjectileType<HungrimoireTeeth>(),
                        20, 5, Projectile.owner);

                    if (proj.ModProjectile is HungrimoireTeeth teeth)
                    {
                        teeth.SetFrame((int)Math.Ceiling(Projectile.ai[0] / 5));
                        proj.spriteDirection = Math.Sign(Projectile.velocity.X);
                    }
                }
            }
        }
    }
}

public class HungrimoireTeeth : EverProjectile
{
    private ParticleLayer Blood = new ParticleLayer();
    
    public override string Texture => "Everware/Assets/Textures/EyeOfCthulhu/Drops/HungrimoireTeeth";

    public Rectangle Hitbox;

    public override void NetOnSpawn()
    {
        base.NetOnSpawn();

        for (int i = 0; i < 3; i++)
        {
            float rnd = Main.rand.NextFloat(0.5f, 1f);
            CthulhuBloodParticle part = new CthulhuBloodParticle(Projectile.Bottom,
                new Vector2(Projectile.spriteDirection * 1, -2).RotatedByRandom(MathHelper.PiOver2) * rnd);

            part.Scale.Y = rnd;
            part.Scale.X = 0.3f;
            part.Pixelated = false;
            
            part.Spawn(Blood);
        }

        for (int i = 0; i < 20; i++)
        {
            Dust.NewDust(Projectile.Bottom + new Vector2(-10, -20), 20, 20, DustID.Blood, Projectile.spriteDirection, -2);
        }
        
        SoundEngine.PlaySound(Assets.Sounds.NPC.EoCTendrilEmerge.Asset.WithPitchVariance(0.2f).WithPitchOffset((float)(Projectile.frame / 15f)).WithVolumeScale((Projectile.frame / 14f)) with {MaxInstances = 7}, Projectile.Center);
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox = Hitbox;
        base.ModifyDamageHitbox(ref hitbox);
    }

    public void SetFrame(int frameNum)
    {
        Projectile.frame = frameNum;

        Projectile.Bottom = Projectile.Bottom.Grounded();
        
        float y1 = Projectile.BottomRight.Grounded().Y;
        float y2 = Projectile.BottomLeft.Grounded().Y;

        Projectile.Bottom = new Vector2(Projectile.Bottom.X, Math.Max(y1, y2));

        Hitbox = new Rectangle((int)Projectile.Bottom.X - 15, (int)Projectile.Bottom.Y - (10 * frameNum), 30, 10 * frameNum);
    
        Projectile.ai[0] = 1f - (frameNum * 0.01f);
    }
    
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.rotation = Main.rand.NextFloat(-0.2f, 0.2f);
        Projectile.DamageType = DamageClass.Magic;
        Projectile.damage = 15;
        Projectile.knockBack = 2;
        Projectile.width = 30;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.height = 80;
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.projFrames[Projectile.type] = 8;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers,
        List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
        base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }

    public override void AI()
    {
        Blood.Update();
        Blood.Update();
        Projectile.ai[1]++;
        if (Projectile.ai[1] < 30)
        {
            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 1f, 0.2f);
        }
        else
        {
            Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 1.05f, -0.1f);
            if (Projectile.ai[0] < 0) Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var asset = Assets.Textures.EyeOfCthulhu.Drops.HungrimoireTeeth.Asset;
        
        Rectangle fr = asset.Frame(8, 1, Projectile.frame);
        
        fr.Height = (int)(fr.Height * Projectile.ai[0]);
        Vector2 origin = new Vector2(fr.Width / 2f, fr.Height);

        float offset = Projectile.spriteDirection > 0
            ? (-10 + (Projectile.ai[0] * 10))
            : (10 - (Projectile.ai[0] * 10));

        float rot = (Projectile.spriteDirection * 0.5f) + (Projectile.spriteDirection * Projectile.ai[0] * -0.5f);
        
        Main.EntitySpriteDraw(asset.Value, Hitbox.Bottom() + new Vector2(offset, 2) - Main.screenPosition, fr, lightColor, Projectile.rotation + rot, origin, 1f, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
        
        Blood.Draw();
        return false;
    }
}