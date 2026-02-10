using Terraria.ID;

namespace Everware.Content.Base.Items;

public static class EverItemExtensions
{
    public static void DefaultToArmor(this Item item, int defense)
    {
        item.wornArmor = true;
        item.defense = defense;
    }
    public static void DefaultToBasicWeapon(this Item item, int damage, int useTime, DamageClass clss)
    {
        item.useTime = useTime;
        item.useAnimation = useTime;
        item.useStyle = ItemUseStyleID.HiddenAnimation;
        item.damage = damage;
        item.DamageType = clss;
        item.noMelee = true;
        item.noUseGraphic = true;
    }
}
