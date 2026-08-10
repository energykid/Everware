using Everware.Common.Systems;
using Everware.Content.Base.Items;
using Everware.Content.Base.ParticleSystem;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Underground.DeepCaveLoot;

public class Whetstone : EverItem
{
    public override int Rarity => ItemRarityID.Pink;
    public static readonly float ParryRange = 1.5f;
    public static readonly int ParryDamage = 38;
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/Whetstone";

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToAccessory(32, 20);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);
        player.GetModPlayer<WhetstonePlayer>().active = true;
    }
}

public class RazorsEdgeBuff : ModBuff
{
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/RazorsEdgeBuff";
}
public class RazorsEdgeCooldownBuff : ModBuff
{
    public override bool RightClick(int buffIndex)
    {
        return false;
    }
    public override string Texture => "Everware/Assets/Textures/Underground/DeepCaveLoot/RazorsEdgeCooldownBuff";
}

public class WhetstonePlayer : ModPlayer
{
    public bool active = false;
    public bool CanParry => active && !Player.HasBuff(ModContent.BuffType<RazorsEdgeCooldownBuff>());
    public override float UseSpeedMultiplier(Item item)
    {
        if (Player.HasBuff(ModContent.BuffType<RazorsEdgeBuff>()) && item.DamageType == DamageClass.Melee)
        {
            return base.UseSpeedMultiplier(item) * 1.2f;
        }

        return base.UseSpeedMultiplier(item);
    }
    public override void ResetEffects()
    {
        active = false;
    }

    public override void PostUpdate()
    {
        if (CanParry && Player.ItemAnimationActive)
        {
            if (!Player.HasBuff(ModContent.BuffType<RazorsEdgeCooldownBuff>()))
            {
                Player.ItemCheck_GetMeleeHitbox(Player.HeldItem, Player.HeldItem.getRect(), out bool dontAttack, out Rectangle itemRect);

                itemRect.X -= 10; itemRect.Y -= 10;
                itemRect.Width += 20; itemRect.Height += 20;

                // true melee parrying
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (itemRect.Contains(proj.Hitbox.Center))
                    {
                        ParryProjectile(proj);
                    }
                }

                // holdout projectile parrying
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj != null)
                    {
                        // any melee projectile can deflect, it just has to be close to the Player to work
                        if (proj.DamageType == DamageClass.Melee && proj.Distance(Player.Center) < proj.Size.Length() * Whetstone.ParryRange && proj.damage > 0)
                        {
                            for (int j = 0; j < Main.projectile.Length; j++)
                            {
                                Projectile parried = Main.projectile[j];
                                if (proj.Hitbox.Contains(parried.Hitbox.Center))
                                    ParryProjectile(parried);
                            }
                        }
                    }
                }
            }
        }
    }
    public void ParryProjectile(Projectile parried)
    {
        // parried projectile needs to be at least 20% smaller than your held item to work
        if (parried.Size.Length() < Player.HeldItem.Size.Length() * 0.8f && parried.hostile && !parried.reflected && parried.active)
        {
            SoundEngine.PlaySound(SoundID.Tink.WithPitchOffset(-0.6f), parried.Center);
            SoundEngine.PlaySound(Assets.Sounds.Gear.Accessory.WhetstoneParry.Asset, parried.Center);
            ScreenEffects.AddScreenShake(parried.Center, 4f);
            parried.hostile = false;
            parried.damage = Whetstone.ParryDamage;
            parried.friendly = true;
            parried.Center = Player.Center + new Vector2(35, 0).RotatedBy(Player.AngleTo(Player.NetworkHandler().MousePosition));
            parried.velocity = new Vector2(10 + (parried.velocity.Length() / 2), 0).RotatedBy(Player.AngleTo(Player.NetworkHandler().MousePosition));
            parried.reflected = true;
            parried.extraUpdates += 1;
            Player.AddBuff(ModContent.BuffType<RazorsEdgeBuff>(), 60 * 6);
            Player.AddBuff(ModContent.BuffType<RazorsEdgeCooldownBuff>(), 60 * 16);

            for (int i = 0; i < 10; i++)
            {
                if (i < 2)
                    new ParryFlash(parried.Center, Vector2.Zero).Spawn();
                new ParrySpark(parried.Center, new Vector2(Main.rand.NextFloat(7f), 0).RotatedByRandom(MathHelper.TwoPi)).Spawn();
            }
        }
    }
}

public class ParrySpark : Particle
{
    public override Asset<Texture2D> Texture => Assets.Textures.Underground.DeepCaveLoot.ParrySpark.Asset;
    public ParrySpark(Vector2 pos, Vector2 vel) : base(pos, vel, Vector2.One, null, null)
    {
        position = pos;
        velocity = vel;
        AffectedByLight = false;
        Pixelated = true;
        Rotation = velocity.AngleFrom(Vector2.Zero);
    }
    public override void Update()
    {
        Scale = new Vector2(velocity.Length() / 2f, 1f) * Opacity;
        Rotation = velocity.AngleFrom(Vector2.Zero);
        velocity.Y += 0.2f;
        Opacity -= 0.08f;
        Lighting.AddLight(position, 0.03f * Opacity, 0.05f * Opacity, 0.1f * Opacity);
        if (Opacity < 0) Kill();
        base.Update();
    }
}
public class ParryFlash : Particle
{
    public override Asset<Texture2D> Texture => Assets.Textures.Underground.DeepCaveLoot.ParryFlash.Asset;
    public ParryFlash(Vector2 pos, Vector2 vel) : base(pos, vel, Vector2.One, null, null)
    {
        FrameCount = new Vector2(4, 1);
        FrameNum.X = 0;
        position = pos;
        velocity = vel;
        AffectedByLight = false;
        Pixelated = true;
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
    }
    public override void Update()
    {
        FrameNum.X += 0.3f;
        if (FrameNum.X >= 4) Kill();
        Rotation += 0.05f;
        float sc = MathHelper.Lerp(1f, 0f, FrameNum.X / 4f);

        Lighting.AddLight(position, 0.1f * sc, 0.2f * sc, 0.4f * sc);
        base.Update();
    }
}