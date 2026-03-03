using Everware.Common.Players;
using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Hell;

class SkullStaff : EverWeaponItem
{
    public override string Texture => "Everware/Assets/Textures/Hell/SkullStaff";
    public override void SetDefaults()
    {
        base.SetDefaults();
        DefaultToSummonWeapon(50, 2f, 20, 10, ModContent.ProjectileType<SkullMinion>(), ModContent.BuffType<SkullMinionBuff>(), 50);
        Item.noMelee = true;
        Item.rare = ItemRarityID.LightRed;
    }
    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
        position = player.GetModPlayer<NetworkPlayer>().MousePosition;
    }
}