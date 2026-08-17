using Everware.Content.Base.Items;
using Everware.Content.Gallery;
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
        return false;
    }
    public override bool? UseItem(Player player)
    {
        if (player.ItemAnimationJustStarted)
        {
            int snowDirection = 0;

            bool success = false;

            Point pos = new Point(Main.spawnTileX, (int)Main.worldSurface + Main.rand.Next(100, 300));

            for (int i = 0; i < Main.maxTilesX / 2; i++)
            {
                Point p1 = pos + new Point(i, 0);
                Point p2 = pos + new Point(-i, 0);

                if (p1.X > 100 && p1.X < Main.maxTilesX - 100)
                {
                    if (Main.tile[p1].TileType == TileID.SnowBlock || Main.tile[p1].TileType == TileID.IceBlock)
                    {
                        pos = p1;
                        snowDirection = 1;
                        success = true;
                        break;
                    }
                }

                if (p2.X > 100 && p2.X < Main.maxTilesX - 100)
                {
                    if (Main.tile[p2].TileType == TileID.SnowBlock || Main.tile[p2].TileType == TileID.IceBlock)
                    {
                        pos = p2;
                        snowDirection = -1;
                        success = true;
                        break;
                    }
                }
            }

            GalleryGeneration.Everware025_SpawnFrozenSculptor((Main.MouseWorld / 16).ToPoint());

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
