using Everware.Content.Base.World;
using Everware.Content.PreHardmode.Kiln.Tiles;
using Everware.Content.PreHardmode.Quarry.Tiles;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace Everware.Content.PreHardmode.Quarry;

public static class QuarryGenerator
{
    public static void GenerateQuarry(Point p)
    {
        Point BasePoint = p;
        #region Pass 1 (Hole and rebar walls)

        // Place rebar walls
        for (int i = -10; i < 10; i++)
        {
            for (int j = -3; j < 25; j++)
            {
                if (Main.tile[BasePoint.X + i, BasePoint.Y + j].HasTile || Main.tile[BasePoint.X + i, BasePoint.Y + j].WallType != WallID.None)
                {
                    WorldGen.KillWall(BasePoint.X + i, BasePoint.Y + j);
                    WorldGen.PlaceWall(BasePoint.X + i, BasePoint.Y + j, ModContent.WallType<RebarRodPlaced>(), true);
                }
            }
        }

        // Create digging hole
        new CustomGenShapes.Hole(10, Main.rand.Next(5, 7)).Perform(BasePoint, Actions.Chain(new CustomGenActions.SetTileFromOther(TileID.Stone), new Actions.Smooth(true)));
        CustomGenShapes.Hole hole = new CustomGenShapes.Hole(6, Main.rand.Next(3, 5));
        CustomGenShapes.Hole hole2 = new CustomGenShapes.Hole(6, hole._passes + 1);
        hole.Perform(BasePoint, new Actions.ClearTile(true));
        hole2.Perform(BasePoint, new Actions.Smooth(true));

        #endregion

        int HousePositionX = Main.rand.NextBool() ? -16 : 16;

        Point HousePoint = new Point(BasePoint.X + HousePositionX, BasePoint.Y);

        #region Pass 2 (Building on the side)

        // Place foundation
        new Shapes.Rectangle(new Rectangle(-7, 2, 14, 8)).Perform(HousePoint,
            new CustomGenActions.SetTileBetweenTwo(TileID.GrayBrick, TileID.Stone)
            );

        // Place main room
        new Shapes.Rectangle(new Rectangle(-7, -6, 14, 8)).Perform(HousePoint,
            new Actions.SetTile(TileID.GrayBrick, true)
            );
        new Shapes.Rectangle(new Rectangle(-7, -5, 14, 5)).Perform(HousePoint,
            Actions.Chain(new CustomGenActions.SetWall(WallID.GrayBrick), new Actions.ClearTile(true))
            );

        // Place worn wood floor
        new Shapes.Rectangle(new Rectangle(-6, 0, 12, 1)).Perform(HousePoint,
            Actions.Chain(new Actions.SetTile((ushort)ModContent.TileType<WornWoodPlaced>(), true)
            ));

        // Place wooden walls
        new Shapes.Rectangle(new Rectangle(-7, -5, 1, 5)).Perform(HousePoint,
            new Actions.SetTile((ushort)ModContent.TileType<WornWoodPlaced>(), true)
            );
        new Shapes.Rectangle(new Rectangle(6, -5, 1, 5)).Perform(HousePoint,
            new Actions.SetTile((ushort)ModContent.TileType<WornWoodPlaced>(), true)
            );

        #endregion
    }
}
