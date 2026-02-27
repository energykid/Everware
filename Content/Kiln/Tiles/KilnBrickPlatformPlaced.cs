using Everware.Content.Kiln.Visual;
using Terraria.ID;
using Terraria.ObjectData;

namespace Everware.Content.Kiln.Tiles;

public class KilnBrickPlatformPlaced : ModTile
{
    public override string Texture => "Everware/Assets/Textures/Kiln/KilnBrickPlatformPlaced";
    public override void SetStaticDefaults()
    {
        // Properties
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        TileID.Sets.Platforms[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
        AddMapEntry(new Color(200, 200, 200));

        DustType = ModContent.DustType<KilnPowderDust>();
        AdjTiles = [TileID.Platforms];
        VanillaFallbackOnModDeletion = TileID.Platforms;

        // Placement
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleMultiplier = 27;
        TileObjectData.newTile.StyleWrapLimit = 27;
        TileObjectData.newTile.UsesCustomCanPlace = false;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);
    }

    public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
}