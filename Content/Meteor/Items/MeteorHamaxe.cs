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
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        Dust dust = Dust.NewDustDirect(player.itemLocation + new Vector2(8, -38).RotatedBy(player.itemRotation), 2, 2, DustID.Torch);
        dust.noGravity = true;
        dust.scale = 1f;
        Dust dust2 = Dust.NewDustDirect(player.itemLocation + new Vector2(8, -38).RotatedBy(player.itemRotation), 2, 2, DustID.Smoke);
        dust2.noGravity = true;
        dust2.scale = 0.8f;
        dust2.color = Color.Black;
        dust2.alpha = 200;
        dust2.velocity = Vector2.Zero;
        base.UseAnimation(player);
    }

    public override bool UseCustomDraw => true;
    public override void CustomDraw(Player player, float direction)
    {
        base.CustomDraw(player, direction);
    }
}