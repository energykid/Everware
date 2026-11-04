using Everware.Config;
using Everware.Content.Base.Items;
using System.Collections.Generic;
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
        return head.type == Type && body.type == ModContent.ItemType<MeteorJacket>() && legs.type == ModContent.ItemType<MeteorBoots>();
    }

    public override void Load()
    {
        On_Player.QuickMount += MountChanges;
    }

    private void MountChanges(On_Player.orig_QuickMount orig, Player self)
    {
        orig(self);
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

public class CruiserKeys : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Items/CruiserKeys";
    public override int DuplicationAmount => 1;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.mountType = ModContent.MountType<MeteorCruiser>();
    }
}

public class MeteorCruiser : ModMount
{
    public override string Texture => "Everware/Assets/Textures/Meteor/Items/MeteorCruiser";

    public override void SetMount(Player player, ref bool skipDust)
    {
        skipDust = true;
        base.SetMount(player, ref skipDust);
    }

    public override void SetStaticDefaults()
    {
        MountID.Sets.FacePlayersVelocity[Type] = true;
        MountData.jumpHeight = 0;
        MountData.flightTimeMax = 1000;
        base.SetStaticDefaults();
    }

    public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer,
        ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition,
        ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation,
        ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
    {
        texture = Assets.Textures.Meteor.Items.MeteorCruiser.Asset.Value;
        return base.Draw(playerDrawData, drawType, drawPlayer, ref texture, ref glowTexture, ref drawPosition, ref frame, ref drawColor, ref glowColor, ref rotation, ref spriteEffects, ref drawOrigin, ref drawScale, shadow);
    }
}