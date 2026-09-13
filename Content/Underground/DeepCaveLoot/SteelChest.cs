using Everware.Content.Base.Items;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;
using TileHelper.Content.Tiles;

namespace Everware.Content.Underground.DeepCaveLoot;

public class SteelChestTile : ChestTile
{
    public override bool RightClick(int i, int j)
    {
        var p16 = TileObjectData.TopLeft(i, j);

        (int, int) topLeft = (p16.X, p16.Y);
        i = topLeft.Item1;
        j = topLeft.Item2;
        Player localPlayer = Main.LocalPlayer;
        Main.mouseRightRelease = false;
        localPlayer.CloseSign();
        localPlayer.SetTalkNPC(-1);
        Main.npcChatCornerItem = 0;
        Main.npcChatText = string.Empty;
        if (Main.editChest)
        {
            SoundEngine.PlaySound(in SoundID.MenuTick);
            Main.editChest = false;
            Main.npcChatText = string.Empty;
        }

        if (localPlayer.editedChestName)
        {
            NetMessage.SendData(33, -1, -1, NetworkText.FromLiteral(Main.chest[localPlayer.chest].name), localPlayer.chest, 1f);
            localPlayer.editedChestName = false;
        }

        bool flag = IsLockedChest(i, j);
        int value;
        if (Main.netMode == 1 && !flag)
        {
            if (i == localPlayer.chestX && j == localPlayer.chestY && localPlayer.chest >= 0)
            {
                localPlayer.chest = -1;
                Recipe.FindRecipes();
                SoundEngine.PlaySound(in SoundID.MenuClose);
            }
            else
            {
                NetMessage.SendData(31, -1, -1, null, i, j);
                Main.stackSplit = 600;
            }
        }
        else if (flag)
        {
            if (localPlayer.ConsumeItem(ItemID.GoldenKey))
            {
                Chest.Unlock(i, j);
            }
        }
        else
        {
            int num = Chest.FindChest(i, j);
            if (num >= 0)
            {
                Main.stackSplit = 600;
                if (num == localPlayer.chest)
                {
                    localPlayer.chest = -1;
                    SoundEngine.PlaySound(in SoundID.MenuClose);
                }
                else
                {
                    SoundEngine.PlaySound((localPlayer.chest < 0) ? SoundID.MenuOpen : SoundID.MenuTick);
                    localPlayer.OpenChest(i, j, num);
                }

                Recipe.FindRecipes();
            }
        }

        return true;
    }
    public override string Texture => "Everware/Assets/Textures/Underground/SteelChestTile";
    public override string HighlightTexture => "Everware/Assets/Textures/Underground/SteelChestTile_Outline";
    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        return [new Item(ModContent.ItemType<SteelChestItem>())];
    }
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        Main.tileShine2[Type] = true;
        Main.tileShine[Type] = 1200;

        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        AddMapEntry(new Color(84, 99, 169));

        MakeLocked(ItemID.GoldenKey);
    }
    public override bool IsLockedChest(int i, int j) => Main.tile[i, j] != null && Main.tile[i, j].TileFrameX > 18;

    public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual)
    {
        return true;
    }
}

public class SteelChestItem : EverPlaceableItem
{
    public override int Rarity => 3;
    public override int PlacementID => ModContent.TileType<SteelChestTile>();
    public override string Texture => "Everware/Assets/Textures/Underground/SteelChestItem";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Sell.Silver(45);
    }
}