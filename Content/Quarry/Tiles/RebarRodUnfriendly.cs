using Terraria.ID;

namespace Everware.Content.Quarry.Tiles;

public class RebarRodPlacedUnfriendly : ModWall
{
    public override string Texture => "Everware/Assets/Textures/Quarry/RebarRodPlaced";
    public override void SetStaticDefaults()
    {
        DustType = DustID.Lead;
        Main.wallLight[Type] = true;
        AddMapEntry(new Color(23, 23, 23));
    }
    public override bool Drop(int i, int j, ref int type)
    {
        return false;
    }
}
