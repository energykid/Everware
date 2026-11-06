using Terraria.Localization;
using Terraria.ObjectData;

namespace Everware.Content.Base.Tiles;

public abstract class MetalBarTile : ModTile
{
    public virtual Color Color => Color.White;
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);

        AddMapEntry(Color, Language.GetText("MapObject.MetalBar"));
    }
}