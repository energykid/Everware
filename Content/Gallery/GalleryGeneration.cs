using Everware.Content.Base.World;
using Everware.Content.Gallery.Sculptor;
using Everware.Utils;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.Gallery;

public class GalleryGeneration
{
    public static void Everware025_SpawnFrozenSculptor(Point center)
    {
        Point surfPoint = (new Point(center.X, (int)Main.worldSurface - 200)).Grounded();
        Point tipPoint = (new Point(center.X, (int)Main.worldSurface - 200)).Grounded() + new Point(0, -6);

        new CustomGenShapes.Triangle(new Point(-7, 20),
            new Point(9, 20),
            new Point(0, -15)).Perform(surfPoint, Actions.Chain(new CustomGenActions.SetTileFromNone(TileID.IceBlock), new Actions.Smooth(true)));

        new Shapes.Rectangle(new Rectangle(-3, -10, 6, 10)).Perform(tipPoint, Actions.Chain(new Actions.Clear(), new Actions.SetFrames(true)));

        WorldGen.PlaceObject(tipPoint.X - 1, tipPoint.Y - 3, ModContent.TileType<FrozenSculptor>());
    }

    /// <summary>
    /// Unused until 0.3.
    /// </summary>
    /// <param name="center"></param>
    public static void GenerateGallery(Point center)
    {
        GallerySystem.GalleryPosition = center;

        // Open dome
        new Shapes.HalfCircle(74).Perform(new Point(center.X, center.Y + 1), new Actions.Clear());

        // Dome walls
        new Shapes.HalfCircle(75).Perform(new Point(center.X, center.Y + 2), new Actions.SetTileKeepWall(TileID.IceBrick, true, true));
        new Shapes.HalfCircle(75).Perform(center, new Actions.SetTileKeepWall(TileID.IceBrick, true, true));

        //Floor
        new Shapes.Rectangle(new Rectangle(-150, -30, 300, 35)).Perform(center, new Actions.SetTileKeepWall(TileID.IceBrick, true, true));

        // Halls
        new Shapes.Rectangle(new Rectangle(-150, -28, 300, 29)).Perform(center, new Actions.Clear());

        // Hall backgrounds
        new Shapes.Rectangle(new Rectangle(-150, -28, 75, 29)).Perform(center, new CustomGenActions.SetWall(WallID.IceBrick));
        new Shapes.Rectangle(new Rectangle(75, -28, 75, 29)).Perform(center, new CustomGenActions.SetWall(WallID.IceBrick));

        // Hall granite backgrounds
        for (int i = 0; i < 3; i++)
        {
            new Shapes.Rectangle(new Rectangle(-75 - (i * 20) - 5, -28, 10, 29)).Perform(center, new CustomGenActions.SetWall(WallID.GraniteBlock));
            new Shapes.Rectangle(new Rectangle(75 + (i * 20) - 5, -28, 10, 29)).Perform(center, new CustomGenActions.SetWall(WallID.GraniteBlock));
        }

        // Clear out dome
        new Shapes.HalfCircle(72).Perform(center, new Actions.ClearTile(true));
        new Shapes.HalfCircle(76).Perform(center, new Actions.Smooth(true));

        // Frozen sculptor
        WorldGen.PlaceObject(center.X, center.Y - 2, ModContent.TileType<FrozenSculptor>());

        // Ice spikes from the ceiling
        for (int i = -45; i <= 45; i += Main.rand.Next(3, 9))
        {
            float rot = i;
            float rotOffset = Main.rand.NextFloat(-15, 15);
            if (Math.Abs(rotOffset) < 4) rotOffset = Math.Sign(rotOffset) * 4f;

            Vector2 p1 = new Vector2(0, -75).RotatedBy(MathHelper.ToRadians(rot));
            Vector2 p2 = new Vector2(0, -75).RotatedBy(MathHelper.ToRadians(rot + rotOffset));
            Vector2 p3 = Vector2.Lerp(p1, p2, 0.5f) + new Vector2(0, Main.rand.NextFloat(10, 20));

            new CustomGenShapes.Triangle(p1.ToPoint(), p2.ToPoint(), p3.ToPoint()).Perform(center, Actions.Chain(new Actions.SetTileKeepWall(TileID.IceBlock, true, true), new Actions.Smooth(true)));
        }
    }
}
