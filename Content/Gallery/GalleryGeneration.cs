using Everware.Content.Base.World;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.Gallery;

public class GalleryGeneration
{
    public static void GenerateGallery(Point center)
    {
        GallerySystem.GalleryPosition = center;

        new Shapes.HalfCircle(74).Perform(new Point(center.X, center.Y + 1), new Actions.Clear());

        new Shapes.HalfCircle(75).Perform(new Point(center.X, center.Y + 2), new Actions.SetTileKeepWall(TileID.IceBrick, true, true));
        new Shapes.HalfCircle(75).Perform(center, new Actions.SetTileKeepWall(TileID.IceBrick, true, true));

        new Shapes.Rectangle(new Rectangle(-150, -30, 300, 35)).Perform(center, new Actions.SetTileKeepWall(TileID.IceBrick, true, true));

        new Shapes.Rectangle(new Rectangle(-150, -28, 300, 29)).Perform(center, new Actions.Clear());

        new Shapes.Rectangle(new Rectangle(-150, -28, 75, 29)).Perform(center, new CustomGenActions.SetWall(WallID.IceBrick));
        new Shapes.Rectangle(new Rectangle(75, -28, 75, 29)).Perform(center, new CustomGenActions.SetWall(WallID.IceBrick));

        for (int i = 0; i < 3; i++)
        {
            new Shapes.Rectangle(new Rectangle(-75 - (i * 20) - 5, -28, 10, 29)).Perform(center, new CustomGenActions.SetWall(WallID.GraniteBlock));
            new Shapes.Rectangle(new Rectangle(75 + (i * 20) - 5, -28, 10, 29)).Perform(center, new CustomGenActions.SetWall(WallID.GraniteBlock));
        }

        new Shapes.HalfCircle(72).Perform(center, Actions.Chain(new Actions.ClearTile(true), new Actions.Smooth(true)));
    }
}
