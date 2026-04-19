using Everware.Content.Base.World;
using System;
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

        new Shapes.HalfCircle(72).Perform(center, new Actions.ClearTile(true));
        new Shapes.HalfCircle(76).Perform(center, new Actions.Smooth(true));

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
