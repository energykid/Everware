using Everware.Common.Players;
using Everware.Content.Base.Items;
using Everware.Utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace Everware.Content.MiscWeapons.Mage;

class UnwieldyStaffLootPool : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.Tim;
    }
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        npcLoot.Add(new OneFromOptionsDropRule(1, 1, [ModContent.ItemType<UnwieldyStaff>()]));
    }
}

class UnwieldyStaff : EverWeaponItem
{
    public override string Texture => "Everware/Assets/Textures/MiscWeapons/UnwieldyStaff";
    public override bool UseCustomDraw => true;
    float angle = 0f;
    float rotation = 0f;
    Vector2 flare = Vector2.Zero;
    float shake = 0f;
    public void ProcessAngle(Player player)
    {
        player.direction = (int)Math.Sign(player.GetModPlayer<NetworkPlayer>().MousePosition.X - player.Center.X);

        rotation = 0f;
        angle = 0f;
        flare = Vector2.Zero;

        float anim = MathHelper.Lerp(1f, 0f, (float)player.itemAnimation / (float)player.itemAnimationMax);

        float a = (3f / (float)player.itemAnimationMax);

        flare = Easing.KeyVector2(anim, 0.75f - a, 0.75f, Vector2.Zero, new Vector2(0.2f, 1f), Easing.InSine, flare);
        flare = Easing.KeyVector2(anim, 0.75f, 0.75f + a, new Vector2(0.2f, 1f), new Vector2(0.7f, 0.7f), Easing.InSine, flare);
        flare = Easing.KeyVector2(anim, 0.75f + a, 0.75f + a + a + a, new Vector2(0.7f, 0.7f), new Vector2(0.7f, 0f), Easing.InOutSine, flare);


        if (player.direction > 0)
        {
            rotation = Easing.KeyFloat(anim, 0.6f, 0.75f, 0, -20, Easing.InCubic, angle);
            rotation = Easing.KeyFloat(anim, 0.75f, 1f, -20, 0, Easing.InOutSine, angle);

            angle = Easing.KeyFloat(anim, 0f, 0.6f, -90, -60, Easing.InOutSine, angle);
            angle = Easing.KeyFloat(anim, 0.6f, 0.75f, -60, -140, Easing.InCubic, angle);
            angle = Easing.KeyFloat(anim, 0.75f, 1f, -140, -90, Easing.InOutSine, angle);
        }
        else
        {
            rotation = Easing.KeyFloat(anim, 0.6f, 0.75f, 0, 20, Easing.InCubic, angle);
            rotation = Easing.KeyFloat(anim, 0.75f, 1f, 20, 0, Easing.InOutSine, angle);

            angle = Easing.KeyFloat(anim, 0f, 0.6f, 90, 60, Easing.InOutSine, angle);
            angle = Easing.KeyFloat(anim, 0.6f, 0.75f, 60, 140, Easing.InCubic, angle);
            angle = Easing.KeyFloat(anim, 0.75f, 1f, 140, 90, Easing.InOutSine, angle);
        }
    }
    public override void CustomDraw(Player player, float direction)
    {
        ProcessAngle(player);

        Asset<Texture2D> tex = Assets.Textures.MiscWeapons.UnwieldyStaff_Held.Asset;
        Asset<Texture2D> texGlow = Assets.Textures.Misc.LensFlash.Asset;

        Vector2 staffPosition = player.MountedCenter + DrawingUtils.PlayerOffset(player) + new Vector2(player.direction == -1 ? 10 : 0, 0) - Main.screenPosition + new Vector2(Main.rand.NextFloat(-shake, shake), 0) + new Vector2(0, 16).RotatedBy(MathHelper.ToRadians(angle));
        Vector2 flarePosition = (staffPosition + new Vector2(-6, -28)).RotatedBy(MathHelper.ToRadians(rotation), staffPosition);

        Main.EntitySpriteDraw(texGlow.Value, flarePosition, texGlow.Frame(), Color.MediumPurple.MultiplyRGBA(new(1f, 1f, 1f, 0f)), 0f, texGlow.Size() / 2, flare, SpriteEffects.None);
        Main.EntitySpriteDraw(tex.Value, staffPosition, tex.Frame(), Lighting.GetColor((player.Center / 16).ToPoint()), -MathHelper.PiOver4 + MathHelper.ToRadians(rotation), new(16, 38), 1f, SpriteEffects.None);
    }
    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        ProcessAngle(player);

        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.ToRadians(angle));

        if (player.itemAnimation == player.itemAnimationMax - 2)
        {
            for (int i = 0; i <= 20; i += 10)
            {
                SoundEngine.PlaySound(Assets.Sounds.Gear.Weapon.UnwieldyStaffCharge.Asset.WithPitchOffset(Main.rand.NextFloat(-0.1f, 0.05f)), player.Center);
                Projectile.NewProjectile(new EntitySource_ItemUse(player, Item, "Unwieldy Staff"), player.GetModPlayer<NetworkPlayer>().MousePosition, Vector2.Zero, ModContent.ProjectileType<ManaSpike>(), Item.damage, 0f, player.whoAmI, player.itemAnimation + i, (float)Math.Floor(player.itemAnimationMax * 0.3f));
            }
        }

        Lighting.AddLight(player.Center, new Vector3(0.6f, 0.4f, 0.75f) * flare.Y);
    }
    public override void SetDefaults()
    {
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.DefaultToBasicWeapon(32, 120, DamageClass.Magic);
        Item.rare = ItemRarityID.Green;
        Item.mana = 10;
    }
}
