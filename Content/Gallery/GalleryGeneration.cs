using Everware.Content.Base.World;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.Gallery;

public class GalleryGeneration
{
    public static void GenerateGallery(Point center)
    {
        GallerySystem.GalleryPosition = center;

        new Shapes.HalfCircle(75).Perform(new Point(center.X, center.Y + 2), new Actions.SetTileKeepWall(TileID.IceBrick, true, true));
        new Shapes.HalfCircle(75).Perform(center, new Actions.SetTileKeepWall(TileID.IceBrick, true, true));
        new Shapes.HalfCircle(74).Perform(new Point(center.X, center.Y + 1), new CustomGenActions.SetWall(WallID.IceBrick));

        new Shapes.Rectangle(new Rectangle(-150, -30, 300, 33)).Perform(center, new Actions.SetTileKeepWall(TileID.IceBrick, true, true));
        new Shapes.Rectangle(new Rectangle(-150, -28, 300, 29)).Perform(center, Actions.Chain(new CustomGenActions.SetWall(WallID.IceBrick), new Actions.ClearTile(true)));

        new Shapes.HalfCircle(72).Perform(center, Actions.Chain(new Actions.ClearTile(true), new Actions.Smooth(true)));
    }
}
