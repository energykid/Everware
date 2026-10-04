using Everware.Config;
using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Meteor.Items;

#region Armor Pieces
[AutoloadEquip(EquipType.Head)]
public class MeteorHelmet : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Items/MeteorHelmet";
    public override int DuplicationAmount => 1;
    public override int Rarity => 5;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToArmor(4);
        Item.value = Sell.Gold(1) + Sell.Silver(25);
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return head.type == ModContent.ItemType<MeteorHelmet>() && body.type == ModContent.ItemType<MeteorJacket>() && legs.type == ModContent.ItemType<MeteorBoots>();
    }
    public override void UpdateArmorSet(Player player)
    {

    }
}
[AutoloadEquip(EquipType.Body)]
public class MeteorJacket : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Items/MeteorJacket";
    public override int DuplicationAmount => 1;
    public override int Rarity => 5;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToArmor(5);
        Item.value = Sell.Gold(1) + Sell.Silver(85);
    }
}
[AutoloadEquip(EquipType.Legs)]
public class MeteorBoots : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Items/MeteorBoots";
    public override int DuplicationAmount => 1;
    public override int Rarity => 5;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.DefaultToArmor(3);
        Item.value = Sell.Gold(1) + Sell.Silver(5);
    }
}
#endregion

public class MeteorArmorMagicSwap : ModSystem
{
    public override void Load()
    {
        On_Item.SetDefaults_int += On_Item_SetDefaults_int;
    }

    private void On_Item_SetDefaults_int(On_Item.orig_SetDefaults_int orig, Item self, int Type)
    {
        if (StyleSettings.MeteorSetEnabled)
        {
            if (Type == ItemID.MeteorHelmet) Type = ModContent.ItemType<MeteorHelmet>();
            if (Type == ItemID.MeteorSuit) Type = ModContent.ItemType<MeteorJacket>();
            if (Type == ItemID.MeteorLeggings) Type = ModContent.ItemType<MeteorBoots>();
        }
        else
        {
            if (Type == ModContent.ItemType<MeteorHelmet>()) Type = ItemID.MeteorHelmet;
            if (Type == ModContent.ItemType<MeteorJacket>()) Type = ItemID.MeteorSuit;
            if (Type == ModContent.ItemType<MeteorBoots>()) Type = ItemID.MeteorLeggings;
        }
        orig(self, Type);
    }
}