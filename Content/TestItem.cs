using Everware.Content.Base.Items;
using Everware.Content.Gallery;
using Everware.Content.Gallery.Sculptor;
using Terraria.ID;

namespace Everware.Content;

public class TestItem : EverItem
{
    public override string Texture => "Everware/Assets/Textures/Misc/TestItem";
    public override bool IsLoadingEnabled(Mod mod)
    {
        return false;
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 20;
    }
    public override bool AltFunctionUse(Player player)
    {
        return false;
    }
    public override bool? UseItem(Player player)
    {
        if (player.ItemAnimationJustStarted)
        {
            Point center = GallerySystem.GalleryPosition;

            WorldGen.PlaceObject(center.X, center.Y - 2, ModContent.TileType<FrozenSculptor>());

            /*
            if (player.altFunctionUse != 2)
            {
                NPC? npc = null;
                if (NPC.CountNPCS(ModContent.NPCType<SculptorNPC>()) != 0) npc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SculptorNPC>())];
                ReliquaryUISystem.OpenTrade(npc);
                ReliquaryUISystem.TradeState.SetDialogue("Hey guys, it's me, the Sculptor! Thanks for watching my videeo's.");
                ReliquaryUISystem.ReliquaryOpenedFromInventory = true;
            }
            else
            {
                ReliquaryUISystem.OpenUI();
                ReliquaryUISystem.ReliquaryOpenedFromInventory = true;
            }
            */
        }

        return base.UseItem(player);
    }
}
