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
        DefaultToSummonWeapon(20, 2f, 20, 10, ModContent.ProjectileType<SkullMinion>(), ModContent.BuffType<SkullMinionBuff>(), 50);
        Item.noMelee = true;
        Item.summon = true;
        Item.rare = ItemRarityID.LightRed;
        Item.width = Assets.Textures.Hell.SkullStaff.Asset.Width();
        Item.height = Assets.Textures.Hell.SkullStaff.Asset.Height();
        Item.value = Sell.Gold(1) + Sell.Silver(50);
    }
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
        ItemID.Sets.LockOnIgnoresCollision[Type] = true;
    }
    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
        position = player.GetModPlayer<NetworkPlayer>().MousePosition;
    }
}