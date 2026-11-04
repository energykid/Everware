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

    public override void UseAnimation(Player player)
    {
        Dust dust = Dust.NewDustDirect(player.itemLocation + new Vector2(18, -36).RotatedBy(player.itemRotation), 0, 0, DustID.Torch);
        dust.noGravity = true;
        dust.noLight = true;
        base.UseAnimation(player);
    }

    public override bool UseCustomDraw => true;
    public override void CustomDraw(Player player, float direction)
    {
        base.CustomDraw(player, direction);
    }
}