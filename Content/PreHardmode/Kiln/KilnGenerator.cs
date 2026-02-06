using Everware.Content.Base.World;
using Everware.Content.PreHardmode.Kiln.Tiles;
using System;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.PreHardmode.Kiln;

public static class KilnGenerator
{
    public static void GenerateKiln(Point p)
    {
        Point BasePoint = new Point(p.X, p.Y);
        int ForgeLocationX = 0;

        ushort WoodFloorType = (ushort)ModContent.TileType<WornWoodPlaced>();

        #region Pass 1 (Base platform)
        // Replace ground with silt in the area
        new Shapes.Slime(22, 1f, 1f).Perform(BasePoint, Actions.Chain(new CustomGenActions.SetSilt(), new Actions.Smooth(true)));

        // Create base platform
        new Shapes.Rectangle(new Rectangle(-10, 0, 20, 2)).Perform(
            BasePoint, new Actions.SetTileKeepWall((ushort)ModContent.TileType<KilnBrickPlaced>(), true));

        // Find location for the forging kiln

        // Find whether the leftmost tile of the platform is elevated, if it is, turn it to a slab then place the forging kiln to the right
        if (PoundTileIfAvailable(BasePoint.X - 10, BasePoint.Y))
        {
            ForgeLocationX = 6;
        }
        // Same as above, but inverse, and if both sides are elevated, place the kiln in the center as if neither were
        if (PoundTileIfAvailable(BasePoint.X + 9, BasePoint.Y))
        {
            if (ForgeLocationX == 6) ForgeLocationX = 0;
            else ForgeLocationX = -7;
        }

        // Place background walls behind the forging kiln
        new Shapes.Rectangle(new Rectangle(-3, -3, 7, 4)).Perform(new Point(BasePoint.X + ForgeLocationX, BasePoint.Y), Actions.Chain(
            new Actions.PlaceWall((ushort)ModContent.WallType<KilnBrickWallPlaced>()),
            new CustomGenActions.ClearTileExcept((ushort)ModContent.TileType<KilnBrickPlaced>())));

        // Place fences outside range of the bricks
        int FencesLeftTillPillar = 1;
        for (int i = -9; i < 9; i++)
        {
            int height = 1;
            if (FencesLeftTillPillar < 1)
            {
                height = Main.rand.Next(2, 6);
                FencesLeftTillPillar = Main.rand.Next(1, 3);
            }
            for (int j = 0; j <= height; j++)
            {
                ushort wallType = (ushort)ModContent.WallType<KilnBrickWallPlaced>();
                ushort fenceType = (ushort)ModContent.WallType<WornFencePlaced>();
                if (Main.tile[BasePoint.X + i, BasePoint.Y - j].WallType == wallType) break;
                if (Main.tile[BasePoint.X + i + 1, BasePoint.Y - j].WallType == wallType) break;
                if (Main.tile[BasePoint.X + i - 1, BasePoint.Y - j].WallType == wallType) break;
                WorldGen.PlaceWall(BasePoint.X + i, BasePoint.Y - j, fenceType);
            }
            FencesLeftTillPillar--;
        }

        Point FloorPoint = new Point(BasePoint.X + ForgeLocationX, BasePoint.Y);

        // Create wooden floor
        if (ForgeLocationX == 0)
        {
            new Shapes.Rectangle(new Rectangle(-9, 0, 7, 1)).Perform(
                FloorPoint, new Actions.SetTileKeepWall(WoodFloorType, true));
            new Shapes.Rectangle(new Rectangle(3, 0, 6, 1)).Perform(
                FloorPoint, new Actions.SetTileKeepWall(WoodFloorType, true));
        }
        else if (ForgeLocationX < 0)
        {
            new Shapes.Rectangle(new Rectangle(3, 0, 12, 1)).Perform(
                FloorPoint, new Actions.SetTileKeepWall(WoodFloorType, true));
        }
        else
        {
            new Shapes.Rectangle(new Rectangle(-14, 0, 10, 1)).Perform(
                FloorPoint, new Actions.SetTileKeepWall(WoodFloorType, true));
        }

        // If the forging kiln is on the left or right side of the platform, place a foreground wall on that side of the platform behind it
        if (ForgeLocationX != 0)
        {
            new Shapes.Rectangle(new Rectangle(-1, -4, 2, 6)).Perform(
                new Point(BasePoint.X + ForgeLocationX + (Math.Sign(ForgeLocationX) * 4), BasePoint.Y), new Actions.SetTileKeepWall((ushort)ModContent.TileType<KilnBrickPlaced>(), true));
        }
        #endregion

        Point ForgeLocationPoint = new Point(BasePoint.X + ForgeLocationX, BasePoint.Y);
        int RoomExtrusionLeft = Main.rand.Next(5, 8);
        int RoomExtrusionRight = Main.rand.Next(5, 8);
        int RoomHeight = Main.rand.Next(4, 6);
        Point RoomBasePoint = new Point(ForgeLocationPoint.X + 1, BasePoint.Y + RoomHeight + 1);

        #region Pass 2 (Base room & kiln itself)

        int bricktype = ModContent.TileType<KilnBrickPlaced>();
        int walltype = ModContent.WallType<KilnBrickWallPlaced>();

        // Set clearForRoom to place walls, clear interior, then smooth walls
        GenAction clearForRoom = Actions.Chain(
            new CustomGenActions.ClearTileForRoom((ushort)bricktype, (ushort)walltype),
            new Actions.Smooth(true));

        // Clear hole for platform
        new Shapes.Rectangle(new Rectangle(-1, 0, 3, 2)).Perform(ForgeLocationPoint, clearForRoom);

        // Place platform
        new Shapes.Rectangle(new Rectangle(-1, 0, 3, 1)).Perform(ForgeLocationPoint, new Actions.SetTileKeepWall((ushort)ModContent.TileType<KilnBrickPlatformPlaced>(), true));

        // Clear top half of room
        new Shapes.Rectangle(new Rectangle(-1 - RoomExtrusionLeft, 2, 3 + RoomExtrusionLeft + RoomExtrusionRight, RoomHeight)).Perform(ForgeLocationPoint, clearForRoom);

        // Clear bottom half of room
        new Shapes.Rectangle(new Rectangle(-1 - RoomExtrusionLeft - 1, 5, 3 + RoomExtrusionLeft + RoomExtrusionRight + 2, RoomHeight - 3)).Perform(ForgeLocationPoint, clearForRoom);

        // Place gray brick walls
        new Shapes.Rectangle(new Rectangle(-1, 0, 3, 1)).Perform(ForgeLocationPoint, Actions.Chain(
                    new Actions.RemoveWall(), new Actions.PlaceWall(WallID.GrayBrick)));

        new Shapes.Rectangle(new Rectangle(-1 - RoomExtrusionLeft, 1, 3 + RoomExtrusionLeft + RoomExtrusionRight, 2)).Perform(ForgeLocationPoint, Actions.Chain(
                    new Actions.RemoveWall(), new Actions.PlaceWall(WallID.GrayBrick)));

        bool BedLeft = Main.rand.NextBool();
        if (ForgeLocationX > 0) BedLeft = false;
        if (ForgeLocationX < 0) BedLeft = true;

        int BedX = RoomBasePoint.X + (BedLeft ? -RoomExtrusionLeft : RoomExtrusionRight - 1) + -1;
        int CookingPotX = RoomBasePoint.X + (BedLeft ? RoomExtrusionRight - 1 : -RoomExtrusionLeft) + (BedLeft ? 0 : -1);
        int BrazierX = ForgeLocationPoint.X + (BedLeft ? 3 : -3);

        // Place bed
        WorldGen.PlaceObject(BedX, RoomBasePoint.Y, TileID.Beds, direction: BedLeft ? 1 : -1);

        // Place cooking pot
        WorldGen.PlaceObject(CookingPotX, RoomBasePoint.Y, TileID.CookingPots, direction: BedLeft ? 1 : -1);

        // Place hanging brazier
        WorldGen.PlaceObject(BrazierX, BasePoint.Y + 2, TileID.BrazierSuspended);

        // Place the Forging Kiln itself
        Point KilnPoint = new Point(BasePoint.X + ForgeLocationX - 2, BasePoint.Y - 4);
        WorldGen.PlaceObject(KilnPoint.X, KilnPoint.Y, ModContent.TileType<ForgingKiln>());

        // Place the workbench next to the Forging Kiln
        int WorkbenchLocation = (ForgeLocationX < 0 ? 3 : -4);
        if (ForgeLocationX == 0)
            WorkbenchLocation = (Main.rand.NextBool() ? 3 : -4);
        WorldGen.PlaceObject(BasePoint.X + ForgeLocationX + WorkbenchLocation, BasePoint.Y - 1, ModContent.TileType<KilnWorkbenchPlaced>());

        #endregion

        // Get rid of item drops (from trees and whatnot)
        for (int i = 0; i < Main.item.Length; i++)
        {
            Main.item[i].active = false;
        }
    }

    /// <summary>
    /// Finds whether or not the given tile is elevated from either of the tiles to the left or right.
    /// If it is, turns it into a slab.
    /// </summary>
    /// <param name="i">The x coordinate of the tile to check.</param>
    /// <param name="j">The y coordinate of the tile to check.</param>
    /// <returns>Whether or not the check results in a slab.</returns>
    public static bool PoundTileIfAvailable(int i, int j)
    {
        if (!Main.tile[new Point(i, j - 1)].HasTile && (!Main.tile[new Point(i - 1, j)].HasTile || !Main.tile[new Point(i + 1, j)].HasTile))
        {
            WorldGen.PoundTile(i, j);
            return true;
        }
        return false;
    }
}
