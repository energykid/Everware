using Everware.Content.Base.Items;
using Everware.Utils;
using Terraria.ID;

namespace Everware.Content.Underground.DeepCaveLoot;

public class MasterLockpick : EverWeaponItem
{
    public override string Texture => "Everware/Assets/Textures/Underground/MasterLockpick";
    public override int Rarity => 3;
    public bool Smashed = false;
    public override bool UseCustomDraw => true;
    public float Rot(Player player)
    {
        float prog = MathHelper.Lerp(0f, 1f, (float)player.itemAnimation / (float)player.itemAnimationMax);
        float rot = 0f;
        rot = Easing.KeyFloat(prog, 0f, 1f, -0.4f, 1f, Easing.InOutExpo, rot);

        if (player.GetEverWeaponItem() is MasterLockpick pick)
        {
            if (pick.Smashed)
                rot = Easing.KeyFloat(prog, 0f, 0.5f, 0.2f, -0.1f, Easing.InBack, rot);
        }

        return rot;
    }
    public override void CustomDraw(Player player, float direction)
    {
        var asset = Assets.Textures.Underground.MasterLockpick.Asset;

        SpriteEffects eff = SpriteEffects.None;
        float dir = 20f;
        dir += (Rot(player) * -180f);

        Vector2 origin = new Vector2(0, asset.Height());

        if (player.direction == -1)
        {
            eff = SpriteEffects.FlipHorizontally;
            dir = -110f;
            dir -= (Rot(player) * -180f);
            origin = asset.Size();
        }

        Vector2 pos = player.Center + new Vector2(0, player.gfxOffY);

        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, dir);

        Main.EntitySpriteDraw(asset.Value, pos - Main.screenPosition, asset.Frame(), Lighting.GetColor((pos / 16).ToPoint()), MathHelper.ToRadians(dir + 45f), origin, 1f, eff);
    }
    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        float dir = -55f;
        dir += (Rot(player) * -180f);

        if (player.direction == -1)
        {
            dir = 55f;
            dir -= (Rot(player) * -180f);
        }

        Vector2 pos = player.Center + new Vector2(0, player.gfxOffY);

        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, MathHelper.ToRadians(dir));
        base.UseStyle(player, heldItemFrame);

        if (player.itemAnimation == (int)(player.itemAnimationMax * 0.65f))
        {
            SoundEngine.PlaySound(SoundID.Item1.WithPitchOffset(-1f), player.MountedCenter, snd =>
            {
                snd.Pitch += 0.1f;
                return snd.IsPlaying;
            });
        }

        if (player.itemAnimation >= player.itemAnimationMax - 2)
        {
            if (player.GetEverWeaponItem() is MasterLockpick pick)
            {
                pick.Smashed = false;
            }
        }

        if (CanHit(player))
        {
            for (int i = 0; i <= 3; i++)
            {
                for (int j = 0; j <= 3; j++)
                {
                    Point p = ((player.Center / 16f) + new Vector2(player.direction * 2, -1) + new Vector2(i - 1, j)).ToPoint();

                    var c = Chest.FindChest(p.X, p.Y);

                    bool ChestViable = (Main.tile[p].TileType == TileID.Containers && Main.tile[p].TileFrameX == (18 * 2 * 2)) || Main.tile[p].TileType == ModContent.TileType<SteelChestTile>();
                    if (ChestViable)
                    {
                        if (Chest.IsLocked(p.X, p.Y))
                        {
                            SoundEngine.PlaySound(SoundID.Item175.WithPitchOffset(-0.3f), player.Center);
                            SoundEngine.PlaySound(SoundID.Item178.WithPitchOffset(-0.5f), player.Center);
                            Chest.Unlock(p.X, p.Y);
                            if (player.GetEverWeaponItem() is MasterLockpick lockpick)
                            {
                                lockpick.Smashed = true;
                            }
                        }
                    }
                }
            }
        }
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToBasicWeapon(6, 20, DamageClass.Generic);
        Item.UseSound = null;
        Item.value = Sell.Gold(1) + Sell.Silver(75);
        Item.width = 36;
        Item.height = 34;
        Item.noMelee = false;
        Item.useTurn = true;
    }
    public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        if (!CanHit(player)) noHitbox = true;
        else noHitbox = false;
        int cenX = (int)player.Center.X + (player.direction * 30);
        hitbox = new Rectangle(cenX - 30, (int)player.Center.Y - 30, 60, 60);
    }
    public bool CanHit(Player player)
    {
        return player.itemAnimation < player.itemAnimationMax / 2 && player.itemAnimation > player.itemAnimationMax / 2.5f;
    }
    public override bool? CanHitNPC(Player player, NPC target)
    {
        if (!CanHit(player))
            return base.CanHitNPC(player, target);
        return true;
    }
    public override bool CanHitPvp(Player player, Player target)
    {
        if (!CanHit(player))
            return base.CanHitPvp(player, target);
        return true;
    }
    public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (target.type == NPCID.Mimic || target.type == NPCID.IceMimic || target.type == NPCID.PresentMimic
            || target.type == NPCID.BigMimicCorruption || target.type == NPCID.BigMimicHallow || target.type == NPCID.BigMimicCrimson)
        {
            modifiers.SourceDamage.Additive += 5;
        }

        if (target.type == NPCID.BigMimicJungle) modifiers.SetInstantKill();

        base.ModifyHitNPC(player, target, ref modifiers);
    }
}
