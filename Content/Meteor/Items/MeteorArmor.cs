using Everware.Config;
using Everware.Content.Base.Items;
using Terraria.ID;

namespace Everware.Content.Meteor.Items;

#region Armor Pieces
[AutoloadEquip(EquipType.Head)]
public class MeteorHelmet : EverEquipmentItem
{
    public override int VanillaID => ItemID.MeteorHelmet;
    public override bool ReplacementCondition => StyleSettings.MeteorSetEnabled;
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
public class MeteorJacket : EverEquipmentItem
{
    public override int VanillaID => ItemID.MeteorSuit;
    public override bool ReplacementCondition => StyleSettings.MeteorSetEnabled;
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
public class MeteorBoots : EverEquipmentItem
{
    public override int VanillaID => ItemID.MeteorLeggings;
    public override bool ReplacementCondition => StyleSettings.MeteorSetEnabled;
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