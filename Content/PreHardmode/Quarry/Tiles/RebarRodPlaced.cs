using Terraria.ID;

namespace Everware.Content.PreHardmode.Quarry.Tiles;

public class RebarRodPlaced : ModWall
{
    public override void SetStaticDefaults()
    {
        DustType = DustID.Lead;
        Main.wallLight[Type] = true;
        AddMapEntry(new Color(23, 23, 23));
    }
}
