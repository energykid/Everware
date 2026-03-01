using Everware.Common.Players;
using Everware.Core.Projectiles;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace Everware.Content.Base.Items;

public abstract class EverWeaponItem : EverItem
{
    public override int DuplicationAmount => 1;

    public override int Rarity => ItemRarityID.Blue;

    public virtual int? HoldoutType => null;

    public virtual int Ammo => AmmoID.None;
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.RemoveAll(TT => { return TT.Name == "BuffTime"; });

        base.ModifyTooltips(tooltips);
    }
    public static int ItemTime(Player player)
    {
        return player.itemAnimationMax - player.itemAnimation;
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
    }
    public override bool? UseItem(Player player)
    {
        if (HoldoutType != null && player.ownedProjectileCounts[(int)HoldoutType] < 1 && player.heldProj == -1 && !Main.dedServ && Main.myPlayer == player.whoAmI)
        {
            var proj = Projectile.NewProjectileDirect(new EntitySource_Misc("Held Projectile"), player.Center, Vector2.Zero, (int)HoldoutType, Item.damage, Item.knockBack, player.whoAmI);

            (proj.ModProjectile as EverHoldoutProjectile).AmmoType = Ammo;
        }
        return base.UseItem(player);
    }
    public override bool Shoot(
        Player player,
        EntitySource_ItemUse_WithAmmo source,
        Vector2 position,
        Vector2 velocity,
        int type,
        int damage,
        float knockback
    )
    {
        if (HoldoutType != null && player.heldProj == -1)
        {
            var proj = Projectile.NewProjectileDirect(new EntitySource_Misc("Held Projectile"), player.Center, Vector2.Zero, (int)HoldoutType, Item.damage, Item.knockBack, player.whoAmI);

            (proj.ModProjectile as EverHoldoutProjectile).AmmoType = Ammo;
            return false;
        }
        return base.Shoot(player, source, position, velocity, type, damage, knockback);
    }
    public void ConsumeAmmo(Player player)
    {
        EntitySource_ItemUse_WithAmmo es = new EntitySource_ItemUse_WithAmmo(player, Item, Ammo);

        Item ammo = player.ChooseAmmo(Item);

        if (ammo.consumable)
            ammo.stack--;
    }
    public void ShootBasic(Player player, Vector2 position)
    {
        if (!Main.dedServ && Main.myPlayer == player.whoAmI)
        {
            if (Ammo != AmmoID.None)
            {
                EntitySource_ItemUse_WithAmmo es = new EntitySource_ItemUse_WithAmmo(player, Item, Ammo);

                Item ammo = player.ChooseAmmo(Item);

                if (ammo.consumable)
                    ammo.stack--;

                Shoot(player, es, position, player.DirectionTo(player.GetModPlayer<NetworkPlayer>().MousePosition) * Item.shootSpeed, ammo.shoot, Item.damage, Item.knockBack);
            }
            else if (Item.shoot != ProjectileID.None)
            {
                EntitySource_ItemUse_WithAmmo es = new EntitySource_ItemUse_WithAmmo(player, Item, AmmoID.None);

                Shoot(player, es, position, player.DirectionTo(player.GetModPlayer<NetworkPlayer>().MousePosition) * Item.shootSpeed, Item.shoot, Item.damage, Item.knockBack);
            }
        }
    }
    public override bool CanUseItem(Player player)
    {
        foreach (Projectile projectile in Main.projectile)
        {
            if (projectile.ModProjectile is EverHoldoutProjectile && projectile.owner == player.whoAmI && projectile.active) return false;
        }

        return base.CanUseItem(player) && player.ownedProjectileCounts[(int)HoldoutType] < 1;
    }
}
