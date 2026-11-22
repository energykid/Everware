using Everware.Common.Colors;
using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Meteor.Items;

public class MeteorHamaxe : EverWeaponItem
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Items/MeteorHamaxe";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.CloneDefaults(ItemID.MeteorHamaxe);
        Item.noUseGraphic = true;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        float rot = player.itemRotation;
        if (player.direction < 0) rot += MathHelper.ToRadians(-45);
        Dust dust2 = Dust.NewDustDirect(player.itemLocation + new Vector2(8, -38).RotatedBy(rot), 2, 2, DustID.Smoke);
        dust2.noGravity = true;
        dust2.scale = 1.8f;
        dust2.color = Color.Black;
        dust2.alpha = 180;
        dust2.velocity *= 0.4f;
        
        dust2.noLight = true;
        Dust dust = Dust.NewDustDirect(player.itemLocation + new Vector2(8, -38).RotatedBy(rot), 2, 2, DustID.Torch);
        dust.noGravity = true;
        dust.noLight = true;
        dust.scale = 1f;
        base.UseAnimation(player);
    }

    public override bool UseCustomDraw => true;
    public override void CustomDraw(Player player, float direction)
    {
        Rectangle fr = Asset.Frame();
        var Glow = Assets.Textures.Meteor.Items.MeteorHamaxe_Glow.Asset;

        Color col = Lighting.GetColor((player.Center / 16).ToPoint());
        Main.EntitySpriteDraw(Asset.Value, player.itemLocation - Main.screenPosition + new Vector2(0, player.gfxOffY), fr, col, player.itemRotation, new Vector2(player.direction == 1 ? 0 : fr.Width, fr.Height), 1f, player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
        Main.EntitySpriteDraw(Glow.Value, player.itemLocation - Main.screenPosition + new Vector2(0, player.gfxOffY), fr, Color.Lerp(Color.OrangeRed.MultiplyRGBA(new(1f, 1f, 1f, 0f)), Color.White, col.Lightness / 255f), player.itemRotation, new Vector2(player.direction == 1 ? 0 : fr.Width, fr.Height), 1f, player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
        
        Lighting.AddLight(player.Center + new Vector2(20, -20).RotatedBy(player.itemRotation), Color.OrangeRed.ToVector3() * 0.5f);
    }
}