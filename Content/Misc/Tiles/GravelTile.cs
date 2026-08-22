using Everware.Content.Base.Items;
using Everware.Content.Base.Tiles;
using Everware.Content.Kiln.Tiles;
using Everware.Content.Quarry.Tiles;
using Terraria.ID;

namespace Everware.Content.Misc.Tiles;

public class GravelTile : EverTile
{
    public override string Texture => "Everware/Assets/Textures/Misc/Tiles/GravelTile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        DustType = DustID.Silt;
        AddMapEntry(new Color(73, 73, 73));
    }
}
public class GravelItem : EverPlaceableItem
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        ItemID.Sets.ExtractinatorMode[Type] = Type;
    }
    public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack)
    {
        resultType = ModContent.ItemType<Kilnstone>();
        if (Main.rand.NextBool(2))
            resultType = ModContent.ItemType<RebarRod>();

        resultStack = Main.rand.Next(1, 3);

        if (extractinatorBlockType == TileID.ChlorophyteExtractinator) resultStack++;

        base.ExtractinatorUse(extractinatorBlockType, ref resultType, ref resultStack);
    }
    public override string Texture => "Everware/Assets/Textures/Misc/Tiles/GravelItem";
    public override int PlacementID => ModContent.TileType<GravelTile>();
}
