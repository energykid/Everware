using Everware.Content.Base.Items;
using Everware.Content.Gallery.Sculptor;
using Everware.Content.Reliquary;
using Terraria.ID;

namespace Everware.Content;

public class TestItem : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Misc/TestItem";
    public override bool IsLoadingEnabled(Mod mod)
    {
        return true;
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 20;
    }
    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
    public override bool? UseItem(Player player)
    {
        if (player.ItemAnimationJustStarted)
        {
            if (player.altFunctionUse != 2)
            {
                NPC? npc = null;
                if (NPC.CountNPCS(ModContent.NPCType<SculptorNPC>()) != 0) npc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SculptorNPC>())];
                ReliquaryUISystem.OpenTrade(npc);
                ReliquaryUISystem.ReliquaryOpenedFromInventory = true;
            }
            else
            {
                ReliquaryUISystem.OpenUI();
                ReliquaryUISystem.ReliquaryOpenedFromInventory = true;
            }
        }

        return base.UseItem(player);
    }
}
