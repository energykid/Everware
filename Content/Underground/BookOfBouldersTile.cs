using System.Collections.Generic;
using Terraria.ID;

namespace Everware.Content.Underground;

public class BookOfBouldersTile : ModTile
{
    public override string Texture => "Everware/Assets/Textures/Underground/BookOfBouldersTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.BlueMoss;
        AddMapEntry(new Color(167, 109, 70));
        Main.tileFrameImportant[Type] = true;
        MineResist = 0.1f;
    }
    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        Main.tile[i, j].TileFrameY = 0;
        Main.tile[i, j].TileFrameX = 0;
        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }
    public override void MouseOver(int i, int j)
    {
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<BookOfBoulders>();
        Main.LocalPlayer.cursorItemIconText = "";
        base.MouseOver(i, j);
    }
    public override bool RightClick(int i, int j)
    {
        WorldGen.KillTile(i, j);
        return true;
    }
    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        return [new Item(ModContent.ItemType<BookOfBoulders>())];
    }
}
