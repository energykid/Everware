using System.Collections.Generic;
using Terraria.ID;

namespace Everware.Content.Underground;

public class CaveBook : ModTile
{
    public override string Texture => "Everware/Assets/Textures/Underground/CaveBooks";
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
        Main.tile[i, j].TileFrameX = (short)((i % 5) * 18);
        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }
    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        return [new Item(ItemID.Book)];
    }
}
