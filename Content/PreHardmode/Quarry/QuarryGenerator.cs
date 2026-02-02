using Everware.Content.Base.World;
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

        // Create digging hole
        new CustomGenShapes.Hole(10, Main.rand.Next(5, 7)).Perform(BasePoint, Actions.Chain(new CustomGenActions.SetTileFromOther(TileID.Stone), new Actions.Smooth(true)));
        CustomGenShapes.Hole hole = new CustomGenShapes.Hole(6, Main.rand.Next(3, 5));
        CustomGenShapes.Hole hole2 = new CustomGenShapes.Hole(6, hole._passes + 1);
        hole.Perform(BasePoint, new Actions.ClearTile(true));
        hole2.Perform(BasePoint, Actions.Chain(new Actions.ClearWall(true), new Actions.Smooth(true)));

        // Place rebar walls
        for (int i = -10; i < 10; i++)
        {
            for (int j = Main.rand.Next(3); j < 25; j++)
            {
                WorldGen.PlaceWall(BasePoint.X + i, BasePoint.Y + j, ModContent.WallType<RebarRodPlaced>(), true);
            }
        }
        #endregion

        #region Pass 2 (Building on the side)

        #endregion
    }
}
