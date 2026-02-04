using Everware.Content.Base.World;
using Everware.Content.PreHardmode.Kiln.Tiles;
using Everware.Content.PreHardmode.Quarry.Tiles;
using System;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.PreHardmode.Quarry;

public static class QuarryGenerator
{
    public static void GenerateQuarry(Point p)
    {
        ushort RebarWallType = (ushort)ModContent.WallType<RebarRodPlaced>();
        ushort BrickType = (ushort)ModContent.TileType<SturdyBricksPlaced>();
        ushort BrickWallType = WallID.GrayBrick;
        ushort WoodType = (ushort)ModContent.TileType<WornWoodPlaced>();
        ushort WoodWallType = TileID.WoodBlock;

        Point BasePoint = p;
        #region Pass 1 (Hole and rebar walls)

        // Place rebar walls
        for (int i = -10; i < 10; i++)
        {
            for (int j = -3; j < 25; j++)
            {
                if (Main.tile[BasePoint.X + i, BasePoint.Y + j].HasTile || Main.tile[BasePoint.X + i, BasePoint.Y + j].WallType != WallID.None)
                {
                    if (new Vector2(BasePoint.X + i, BasePoint.Y + (j / 1.4f)).Distance(BasePoint.ToVector2()) < 10)
                    {
                        WorldGen.KillWall(BasePoint.X + i, BasePoint.Y + j);
                        WorldGen.PlaceWall(BasePoint.X + i, BasePoint.Y + j, RebarWallType, true);
                    }
                }
            }
        }

        // Create digging hole
        new CustomGenShapes.Hole(10, Main.rand.Next(5, 7)).Perform(BasePoint, Actions.Chain(new CustomGenActions.SetTileFromOther(TileID.Stone), new Actions.Smooth(true)));
        CustomGenShapes.Hole hole = new CustomGenShapes.Hole(6, Main.rand.Next(3, 5));
        CustomGenShapes.Hole hole2 = new CustomGenShapes.Hole(6, hole._passes + 1);
        hole.Perform(BasePoint, new Actions.ClearTile(true));
        hole2.Perform(BasePoint, new Actions.Smooth(true));

        // Place platforms on top of quarry
        // but first do this long winded platform position setting code so that the platforms are on solid ground
        Point PlatformPos1 = BasePoint;
        PlatformPos1.X -= 7;
        PlatformPos1 = GroundPoint(PlatformPos1);
        Point PlatformPos2 = BasePoint;
        PlatformPos2.X += 7;
        PlatformPos2 = GroundPoint(PlatformPos2);
        // ok now do the thingy
        WorldGen.SlopeTile(PlatformPos1.X, PlatformPos1.Y + 1, (int)SlopeType.Solid);
        WorldGen.SlopeTile(PlatformPos2.X, PlatformPos2.Y + 1, (int)SlopeType.Solid);
        new Shapes.Rectangle(new Rectangle(1, 1, 3, 1)).Perform(PlatformPos1, new Actions.PlaceTile(TileID.Platforms));
        new Shapes.Rectangle(new Rectangle(-3, 1, 3, 1)).Perform(PlatformPos2, new Actions.PlaceTile(TileID.Platforms));

        #endregion

        int HousePositionX = Main.rand.NextBool() ? -20 : 20;

        Point HousePoint = new Point(BasePoint.X + HousePositionX, BasePoint.Y);

        Point HP1 = GroundPoint(new Point(HousePoint.X - 4, HousePoint.Y));
        Point HP2 = GroundPoint(new Point(HousePoint.X + 4, HousePoint.Y));

        HousePoint.Y = HP1.Y < HP2.Y ? HP1.Y : HP2.Y;
        HousePoint.Y -= 1;

        #region Pass 2 (Building on the side)

        // Place foundation
        new Shapes.Rectangle(new Rectangle(-7, 2, 14, 10)).Perform(HousePoint,
            new CustomGenActions.SetTileBetweenTwo(TileID.GrayBrick, TileID.Stone)
            );

        // Place main room
        new Shapes.Rectangle(new Rectangle(-7, -6, 14, 8)).Perform(HousePoint,
            new Actions.SetTile(BrickType, true)
            );
        new Shapes.Rectangle(new Rectangle(-7, -5, 14, 2)).Perform(HousePoint,
            Actions.Chain(new CustomGenActions.SetWallBetweenTwo(BrickWallType, WallID.Rocks1Echo), new Actions.ClearTile(true))
            );
        new Shapes.Rectangle(new Rectangle(-7, -3, 14, 3)).Perform(HousePoint,
            Actions.Chain(new CustomGenActions.SetWall(BrickWallType), new Actions.ClearTile(true))
            );

        // Clear trees above room
        new Shapes.Rectangle(new Rectangle(-7, -12, 14, 6)).Perform(HousePoint,
            new Actions.Clear()
            );

        // Place worn wood floor
        new Shapes.Rectangle(new Rectangle(-6, 0, 12, 1)).Perform(HousePoint,
            Actions.Chain(new Actions.SetTile(WoodType, true)
            ));

        // Place wooden walls
        new Shapes.Rectangle(new Rectangle(-7, -5, 1, 5)).Perform(HousePoint,
            new Actions.SetTile(WoodType, true)
            );
        new Shapes.Rectangle(new Rectangle(6, -5, 1, 5)).Perform(HousePoint,
            new Actions.SetTile(WoodType, true)
            );

        int SkipDoor = 0;
        // If <0, don't place the left door; if >0, don't place the right door
        // If 0, place both

        if (HousePoint.X < BasePoint.X && GetBlocksPastDoor(new Point(HousePoint.X - 8, HousePoint.Y - 2)) > 0) SkipDoor = -1;
        if (HousePoint.X > BasePoint.X && GetBlocksPastDoor(new Point(HousePoint.X + 7, HousePoint.Y - 2)) > 0) SkipDoor = 1;

        if (SkipDoor != -1)
        {
            new Shapes.Rectangle(new Rectangle(-7, -3, 1, 3)).Perform(HousePoint,
                new Actions.ClearTile(true)
                );
            WorldGen.PlaceDoor(HousePoint.X - 7, HousePoint.Y - 2, TileID.ClosedDoor);
        }
        if (SkipDoor != 1)
        {
            new Shapes.Rectangle(new Rectangle(6, -3, 1, 3)).Perform(HousePoint,
                new Actions.ClearTile(true)
                );
            WorldGen.PlaceDoor(HousePoint.X + 6, HousePoint.Y - 2, TileID.ClosedDoor);
        }

        // Place roof
        new Shapes.Rectangle(new Rectangle(-8, -7, 16, 2)).Perform(HousePoint,
            new Actions.SetTile(BrickType, true)
            );
        new Shapes.Rectangle(new Rectangle(-7, -8, 7, 1)).Perform(HousePoint,
            Actions.Chain(new Actions.SetTile(BrickType, true), new CustomGenActions.PoundTile())
            );
        if (SkipDoor != 1)
            WorldGen.PoundTile(HousePoint.X + 7, HousePoint.Y - 7);

        // Place rebar-cross windows
        new Shapes.Rectangle(new Rectangle(-5, -3, 10, 2)).Perform(HousePoint,
            Actions.Chain(new CustomGenActions.SetWall(RebarWallType), new Actions.ClearTile(true))
            );

        // Place lanterns
        WorldGen.PlaceObject(HousePoint.X - 5, HousePoint.Y - 5, TileID.HangingLanterns);
        WorldGen.PlaceObject(HousePoint.X + 4, HousePoint.Y - 5, TileID.HangingLanterns);

        #endregion
    }

    public static Point GroundPoint(Point p)
    {
        Point BasePoint = p;

        for (int i = 0; i < 30; i++)
        {
            BasePoint.Y++;
            if (WorldGen.SolidOrSlopedTile(Main.tile[BasePoint])) break;
        }
        for (int i = 0; i < 60; i++)
        {
            BasePoint.Y--;
            if (!WorldGen.SolidOrSlopedTile(Main.tile[BasePoint])) break;
        }

        return BasePoint;
    }

    public static int GetBlocksPastDoor(Point start)
    {
        int num = 0;
        for (int i = 0; i < 3; i++)
        {
            if (WorldGen.SolidOrSlopedTile(Main.tile[start.X, start.Y + i])) num += 1;
        }
        return num;
    }
}
