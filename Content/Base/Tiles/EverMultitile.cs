using Terraria.ObjectData;

namespace Everware.Content.Base.Tiles;

public abstract class EverMultitile : EverTile
{
    public virtual int Width => 2;
    public virtual int Height => 2;
    public virtual TileObjectData DefaultStyle => TileObjectData.Style1x2;
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(DefaultStyle);
        TileObjectData.newTile.Width = Width;
        TileObjectData.newTile.Height = Height;

        TileObjectData.newTile.CoordinateHeights = new int[Height];
        for (int i = 0; i < Height; i++)
        {
            TileObjectData.newTile.CoordinateHeights[i] = 16;
        }

        TileObjectData.newTile.DrawYOffset = 2;

        TileObjectData.addTile(Type);
    }
}
