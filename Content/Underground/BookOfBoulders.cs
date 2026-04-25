using Everware.Common.Players;
using Everware.Content.Base.Items;
using Everware.Utils;
using Terraria.DataStructures;
using Terraria.ID;

namespace Everware.Content.Underground;

public class BookOfBoulders : EverWeaponItem
{
    public override string Texture => "Everware/Assets/Textures/Underground/BookOfBoulders";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToMagicWeapon(0, 30, 0, true);
        Item.mana = 10;
        Item.shoot = ModContent.ProjectileType<ConjuredBoulder>();
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.shootSpeed = 1;
        Item.damage = 30;
        Item.value = Sell.Gold(1) + Sell.Silver(50);
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Vector2 pos = player.GetModPlayer<NetworkPlayer>().MousePosition + new Vector2(0, -120);
        for (int i = 0; i < 2; i++)
        {
            if (Collision.SolidTiles(pos, 2, 120))
            {
                pos = pos.Grounded() + new Vector2(0, -120);
            }
        }

        Projectile.NewProjectile(source, pos, new(0), ModContent.ProjectileType<ConjuredBoulder>(), damage, knockback, player.whoAmI);
        return false;
    }
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
}
